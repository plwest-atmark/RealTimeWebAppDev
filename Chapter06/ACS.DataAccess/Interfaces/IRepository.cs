using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ASC.DataAccess.Interfaces
{
    /// <summary>
    /// This will be our Respository Interface for the Azure Storage.  This is a more complex example than the previous lesson, but we
    /// already know in advance what we will want to manage with this kind of repository. The TableEntity will be
    /// the only thing that this Repository can be made to manage.
    /// 
    /// As all repositories, we have basic commands of Add, Delete, Find(Get) has already been studied. However, with
    /// this one, we have an additional methods, Update and CreateTable.  
    /// 
    /// Update: 
    ///     With databases, we sometimes need to change data without
    ///     getting rid of it. This is what update is meant to do.  The ability to change data without deleting and
    ///     adding new data is desired. 
    /// 
    /// CreateTable:
    ///     Data needs a place to be stored. Tables can be considered like excel documents on a very large scale.
    ///     They have to have columns and each of the columns store data specific to the name of the column. For example,
    ///     a table for members would have columns for FirstName, LastName, EmailAddress, etc... each would keep the
    ///     specific data for a member and a "row" would be all the data of the memeber.
    ///     
    /// We are using Asyncronous methods so we have to return a Task that we can "await"
    /// for it to be completed at another time. This means we can do multithreaded operations
    /// with the repositories that implement this interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRepository<T> where T : TableEntity
    {
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<T> FindAsync(string partitionKey, string rowKey);
        Task<IEnumerable<T>> FindAllAsync(string partitionkey);
        Task CreateTableAsync();
    }
}
