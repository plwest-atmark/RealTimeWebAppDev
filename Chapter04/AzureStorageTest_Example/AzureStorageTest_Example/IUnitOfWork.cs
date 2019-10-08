using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageTest_Example
{

    /// <summary>
    /// The interface for our Unit of Work. This will be used to create a Unit of Work class that
    /// will manage all transactions on the database. Upon failure, the Queue of RollbackActions should
    /// be executed to ensure the database returns to a normal state.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        Queue<Task<Action>> RollbackActions { get; set; }
        string ConnectionString { get; set; }
        IRepository<T> Repository<T>() where T : TableEntity;
        void CommitTransaction();
    }
}
