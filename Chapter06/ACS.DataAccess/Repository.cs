using ASC.DataAccess.Interfaces;
using ASC.Models.BaseTypes;
using ASC.Utilities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ASC.DataAccess
{
    public class Repository<T> : IRepository<T> where T : TableEntity, new()
    {
        private readonly CloudStorageAccount storageAccount;
        private readonly CloudTableClient tableClient;
        private readonly CloudTable storageTable;

        public IUnitOfWork Scope { get; set; }

        public Repository(IUnitOfWork scope)
        {
            // Get the Azure Account based on the connection string from the Unit of Work Scope
            storageAccount = CloudStorageAccount.Parse(scope.ConnectionString);

            // Create the client to be used for this Unit of Work
            tableClient = storageAccount.CreateCloudTableClient();
            // Get the table reference for the repository in question.  This will basically
            // be a representation of the "type" (T), in the form of an Azure Storage Table.
            CloudTable table = tableClient.GetTableReference(typeof(T).Name);

            // set the table and scope for this repository.
            this.storageTable = table;
            this.Scope = scope;
        }

        /// <summary>
        /// Method to create an "Add"(Insert) entry into the database.
        /// 
        /// This will use the ExecuteAsync method to actually do the work.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<T> AddAsync(T entity)
        {
            var entityToInsert = entity as BaseEntity;
            entityToInsert.CreatedDate = DateTime.UtcNow;
            entityToInsert.UpdatedDate = DateTime.UtcNow;

            TableOperation insertOperation = TableOperation.Insert(entity);
            var result = await ExecuteAsync(insertOperation);
            return result.Result as T;
        }


        /// <summary>
        /// Method to create an "Update"(Replace) entry into the database
        /// 
        /// This will use the ExecuteAsync method to actually do the work.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<T> UpdateAsync(T entity)
        {
            var entityToUpdate = entity as BaseEntity;
            entityToUpdate.UpdatedDate = DateTime.UtcNow;

            TableOperation updateOperation = TableOperation.Replace(entity);
            var result = await ExecuteAsync(updateOperation);
            return result.Result as T;
        }

        /// <summary>
        /// Method to create a "Delete"(Delete) entry into the database
        /// 
        /// This will use the ExecuteAsync method to actually do the work.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task DeleteAsync(T entity)
        {
            var entityToDelete = entity as BaseEntity;
            entityToDelete.UpdatedDate = DateTime.UtcNow;
            entityToDelete.IsDeleted = true;

            //! NOTE, the book says TableOperation.Replace...
            //! This should be TableOperation.Delete instead as we are creating a delete operation.
            TableOperation deleteOperation = TableOperation.Delete(entityToDelete);
            await ExecuteAsync(deleteOperation);
        }


        /// <summary>
        /// Our generic method to execute operations that we created in the
        /// DeleteAsync, UpdateAsync and AddAsync methods.  This is a way to reuse the
        /// same code without having to place it in every one of the above methods as
        /// the process is the same for all three.
        /// 
        /// Create a rollback action and add it to the scope of the Unit of Work
        /// Execute the action that was created. Either an Add, Delete or Update.
        /// 
        /// Then if the operation "entity" has had the IAuditTracker interface
        /// implemented, it will create an audit record. Again, we will create
        /// a rollback action of the audit operations in case of failure. We want
        /// all aspects of the database to be consistent, so this will also be rollback
        /// if necessary.
        /// 
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        private async Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            var rollbackAction = CreateRollbackAction(operation);
            var result = await storageTable.ExecuteAsync(operation);
            Scope.RollbackActions.Enqueue(rollbackAction);

            // Audit Implementation
            if (operation.Entity is IAuditTracker)
            {
                // Make sure we do not use same RowKey and PartitionKey
                var auditEntity = ObjectExtension.CopyObject<T>(operation.Entity);
                auditEntity.PartitionKey = $"{auditEntity.PartitionKey}-{auditEntity.RowKey}";
                auditEntity.RowKey = $"{DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff")}";

                var auditOperation = TableOperation.Insert(auditEntity);
                var auditRollbackAction = CreateRollbackAction(auditOperation, true);

                var auditTable = tableClient.GetTableReference($"{typeof(T).Name}Audit");
                await auditTable.ExecuteAsync(auditOperation);

                Scope.RollbackActions.Enqueue(auditRollbackAction);
            }

            return result;
        }

        /// <summary>
        /// Method for retrieving ONE item from the database determined by the
        /// PartitionKey and RowKey for that item.
        /// 
        /// Both are needed to find the correct item.
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
        /// <returns></returns>
        public async Task<T> FindAsync(string partitionKey, string rowKey)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var result = await storageTable.ExecuteAsync(retrieveOperation);
            return result.Result as T;
        }


        /// <summary>
        /// Method for retrieving ALL items from the database with a specific
        /// PartionKey.
        /// </summary>
        /// <param name="partitionkey"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> FindAllAsync(string partitionkey)
        {
            TableQuery<T> query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionkey));
            TableContinuationToken tableContinuationToken = null;
            var result = await storageTable.ExecuteQuerySegmentedAsync(query, tableContinuationToken);
            return result.Results as IEnumerable<T>;
        }

        /// <summary>
        /// Method to create tables based on the type T
        /// 
        /// Only creates a table if that table does not already exists. Also
        /// creates an Audit table if the IAuditTracker interface is implemented
        /// on that type.
        /// </summary>
        /// <returns></returns>
        public async Task CreateTableAsync()
        {
            CloudTable table = tableClient.GetTableReference(typeof(T).Name);
            await table.CreateIfNotExistsAsync();

            if (typeof(IAuditTracker).IsAssignableFrom(typeof(T)))
            {
                var auditTable = tableClient.GetTableReference($"{typeof(T).Name}Audit");
                await auditTable.CreateIfNotExistsAsync();
            }
        }

        /// <summary>
        /// If our "transaction" fails in our unit of work then we want to create operations
        /// that will undo what was done to the database.  This will ensure that partial or false data
        /// is not in the database.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="IsAuditOperation"></param>
        /// <returns></returns>
        private async Task<Action> CreateRollbackAction(TableOperation operation, bool IsAuditOperation = false)
        {
            //? If our operation is simply to retrieve data, then there is no reason to create
            //? a rollback operation.  Nothing in the database has been changed.
            if (operation.OperationType == TableOperationType.Retrieve) return null;

            //? Get the table that is used during this operation
            var tableEntity = operation.Entity;

            //? If we are using audit information, then we also want to get the audit table which
            //? is named the same as the type (T) with Audit attached. In other words a Book object
            //? would have an audit table of BookAudit
            var cloudTable = !IsAuditOperation ? storageTable : tableClient.GetTableReference($"{typeof(T).Name}Audit");

            //? Depending on the operation, we want to create different "undo" operations.
            switch (operation.OperationType)
            {
                case TableOperationType.Insert: //! If this is an insert operation
                    return async () => await UndoInsertOperationAsync(cloudTable, tableEntity);

                case TableOperationType.Delete://! If this is an delete operation
                    return async () => await UndoDeleteOperation(cloudTable, tableEntity);

                case TableOperationType.Replace: //! If this is an replace operation
                    var retrieveResult = await cloudTable.ExecuteAsync(TableOperation.Retrieve(tableEntity.PartitionKey, tableEntity.RowKey));
                    return async () => await UndoReplaceOperation(cloudTable, retrieveResult.Result as DynamicTableEntity, tableEntity);

                default: //! We want a default in case there is an operation that is not supported. 
                    throw new InvalidOperationException("The storage operation cannot be identified.");
            }
        }

        /// <summary>
        /// If we have an insert, then we create a delete operation and execute that operation.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task UndoInsertOperationAsync(CloudTable table, ITableEntity entity)
        {
            var deleteOperation = TableOperation.Delete(entity);
            await table.ExecuteAsync(deleteOperation);
        }


        /// <summary>
        /// If we have a delete operation, we want to create an insert and execute that operation.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task UndoDeleteOperation(CloudTable table, ITableEntity entity)
        {
            var entityToRestore = entity as BaseEntity;
            entityToRestore.IsDeleted = false; //? remember to set the "IsDeleted" flag to false since we are undoing the delete.

            var insertOperation = TableOperation.Replace(entity);
            await table.ExecuteAsync(insertOperation);
        }

        /// <summary>
        /// With the replace, we need to original entity that was replaced, so we can restore it as it was before.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="originalEntity"></param>
        /// <param name="newEntity"></param>
        /// <returns></returns>
        private async Task UndoReplaceOperation(CloudTable table, ITableEntity originalEntity, ITableEntity newEntity)
        {

            // we cannot undo a replace if we do not have the original entity that we replaced.
            if (originalEntity != null)
            {
                // Ensure the new Entity ETag is not null or empty and if it is not, we the
                // original ETag to the new ETag.
                if (!String.IsNullOrEmpty(newEntity.ETag)) originalEntity.ETag = newEntity.ETag;


                // perform the operation
                var replaceOperation = TableOperation.Replace(originalEntity);
                await table.ExecuteAsync(replaceOperation);
            }
        }
    }
}
