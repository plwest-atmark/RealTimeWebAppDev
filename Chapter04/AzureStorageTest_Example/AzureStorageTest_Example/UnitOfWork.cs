using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageTest_Example
{
 
    public class UnitOfWork : IUnitOfWork
    {
        //? Used to determine of this unit of work is disposed. This is due to the IDisposable interface use.
        private bool disposed;
        //? flag used to determine if a unit of work completed. We check this when
        //? we "dispose" the unit of work, and if we did not complete the transaction we
        //? will preform the rollback operations.
        private bool complete;
        //? The connection string for the Azure Storage, this will be gotten from the configuration
        //? and injected into the constructor of the unit of work.
        public string ConnectionString { get; set; }
        //? Our unit of work will store all repositories(of objects) that will be used
        //? in a given transaction or "unit of work".  This allows the unit of work to 
        //? be independent of anything else and handle all repository actions.
        private Dictionary<string, object> _repositories;
        //? For each of our "actions" we will create a "rollback" action in case of transaction failure.
        //? We will use a queue so first in / first out operations will be performed in the correct order.
        public Queue<Task<Action>> RollbackActions { get; set; }

        /// <summary>
        /// The constructor of our unit of work. This will allow for a connection string to be passed
        /// based on what Azure Storage we want to use for this Unit of Work.
        /// </summary>
        /// <param name="connectionString"></param>
        public UnitOfWork(string connectionString)
        {
            ConnectionString = connectionString;
            RollbackActions = new Queue<Task<Action>>();
        }


        /// <summary>
        /// At the end of the unit of work, we will "commit" the transaction and ensure that we set
        /// the complete flag to true. Then when the Unit of Work is disposed, we check to ensure
        /// that this was set. If it has not been set on disposal, that means that our transaction has
        /// failed during some part of the operations and the rollback actions should be performed.
        /// </summary>
        public void CommitTransaction()
        {
           // if (disposed) throw new ObjectDisposedException("TransactionScope");
            complete = true;
        }

        /// <summary>
        /// Our "destructor", when the Unit of Work is finished, this will be called. This then calls
        /// the dispose method to ensure that we
        /// </summary>
        ~UnitOfWork()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            //? if already disposed, we do not want to perform these actions again.
            if (!disposed)
            {
                //? ON the first time, set this to true so it will not happen again.
                disposed = true;

                //? If this is being disposed
                if (disposing)
                {
                    try
                    {
                        //? Ensure that this has been completed. If this has NOT been completed,
                        //? we want to RollBackTransactions that were performed in this Unit of Work
                        if (!complete) RollbackTransaction();
                    }
                    finally
                    {
                        //? When finished we will clear the RollbackActions Queue so we can reuse the same
                        //? Unit of Work for different transactions without having to create new ones.
                        //? This is also another reason we are ensuring to call the SuppressFinalize
                        RollbackActions.Clear();
                    }
                }

                //? once we have disposed, set this flag to complete, this means we are done.
                complete = false;
            }
        }


        /// <summary>
        /// Used by the system upon disposal of the Unit of Work. This will 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            //! Objects that implement the IDisposable interface can call this method from the object's 
            //! IDisposable.Dispose implementation to prevent the garbage collector from calling Object.Finalize 
            //! on an object that does not require it. Typically, this is done to prevent the finalizer from 
            //! releasing unmanaged resources that have already been freed by the IDisposable.Dispose implementation.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Method for performing the "Rollback" actions that have been stored in the queue.
        /// This will be done if at any point the "Unit of Work" failed in the transaction scope.
        /// </summary>
        private void RollbackTransaction()
        {
            while (RollbackActions.Count > 0)
            {
                var undoAction = RollbackActions.Dequeue();
                undoAction.Result();
            }
        }


        /// <summary>
        /// This is a "method" that we are using like a property. This first
        /// checks to see if a repository of that kind already exists. If it does
        /// exist, we return that repository.  If it does not exist, we create
        /// a new repository of that type and add it to the dictionary.  Then return
        /// the new respository that we created in this method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IRepository<T> Repository<T>() where T : TableEntity
        {

            // check to ensure that we have initialized our private dictionary.
            if (_repositories == null)
                _repositories = new Dictionary<string, object>();

            // get the "name" of the type that will be used as the key to the dictionary.
            var type = typeof(T).Name;


            // check for the current type of repository and if found return it.
            if (_repositories.ContainsKey(type)) return (IRepository<T>)_repositories[type];

            // the next two lines are a way to create a generic type of object.
            // The Activator.CreateInstance method creates an instance of a specified type 
            // using the constructor that best matches the specified parameters.

            // since we are using T, we cannot just use "new Repository<T>(this); as it is generic.
            // we must use the Activator.CreateInstance to create a new generic type.
            var repositoryType = typeof(Repository<>);
            var repositoryInstance =
                Activator.CreateInstance(repositoryType
                    .MakeGenericType(typeof(T)), this);


            // once created, we will add this new repository to our dictionary
            _repositories.Add(type, repositoryInstance);
            // return the new repository that was just created.
            return (IRepository<T>)_repositories[type];
        }
    }
}
