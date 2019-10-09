using AzureStorageTest_Example;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureStorageTest_Example
{
    /// <summary>
    /// Using our Base Entity we want to create a class that we will be storing in the database.  
    /// 
    /// This is simply a model of something we want to store in the database.  The RowKey and PartitionKey
    /// are how Azure Storage manages the identity of the items within the database. So, in this case we
    /// will assign each book an identification number for the RowKey and use the publisher as the PartitionKey.
    /// 
    /// PartitionKeys are used to provide a means to manage "where" an item is stored in Azure Storage.  If we
    /// use the same PartionKey for everything, then everything will be stored on the same partition in storage and
    /// efficiency of the system may be diminished.  Effective use of PartitionKeys are beyond the scope of this
    /// class, but are important for managing large datasets.
    /// 
    /// RowKeys are the "primary" keys of a database table. This means that for each item (of the same type), a unique
    /// RowKey must be generated. This will be the way we identify and get specific items into and out of the database.
    /// 
    /// </summary>
    public class Book : BaseEntity, IAuditTracker
    {
        public Book()
        {
        }

        public Book(int bookid, string publisher)
        {
            this.RowKey = bookid.ToString();
            this.PartitionKey = publisher;
        }

        public int BookId
        {
            get { return Int32.Parse(base.RowKey); }
            set { base.RowKey = value.ToString(); }
        }
        public string BookName { get; set; }
        public string Author { get; set; }
        public string Publisher
        {
            get { return base.PartitionKey; }
            set { base.PartitionKey = value; }
        }
    }
}
