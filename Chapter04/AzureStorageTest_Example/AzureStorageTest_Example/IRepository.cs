using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageTest_Example
{
    /// <summary>
    /// This will be our Respository Interface for the Azure Storage.
    /// 
    /// We are using Asyncronous methods so we have to return a Task that we can "await"
    /// for it to be completed at another time. This means we can do multithreaded operations
    /// with the repositories that implement this interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRepository<T> where T : TableEntity
    {
        // C - Create an entry in the database.
        Task<T> AddAsync(T entity);

        // R - Retrieve an entry from the database
        Task<T> FindAsync(string partitionKey, string rowKey);
        // R - Retrieve many entries from the database
        Task<IEnumerable<T>> FindAllAsync(string partitionkey);

        // U - Update an entry in the database
        Task<T> UpdateAsync(T entity);

        // D - Delete an entry from the database
        Task DeleteAsync(T entity);



        Task CreateTableAsync();
    }
}
