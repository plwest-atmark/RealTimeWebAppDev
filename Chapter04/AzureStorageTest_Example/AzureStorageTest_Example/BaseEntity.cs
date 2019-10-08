using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureStorageTest_Example
{

    /// <summary>
    /// The BaseEntity extends the Microsoft.WindowsAzure.Storage.Table.TableEntity class to include the following:
    /// 
    /// IsDeleted - This will be used to track if an entity has been deleted.
    /// CreatedDate - This will be used to track when the entity was created.
    /// UpdatedDate - This will be used to track if/when an entity has been updated.
    /// CreatedBy - This will be used to track who created the entity. This will be the currently logged in user.
    /// UpdatedBy - This will be used to track who updated the entity. This will be the currently logged in user.
    /// 
    /// We are using a BaseEntity that extends the TableEntity that is provided by Azure Storage becuase we want
    /// to allow for auditing and soft deletes from the database.  This means that we can manage our data more
    /// effectively and give the application the ability to provide historical records of data changes along with
    /// metrics on data usage and by which user.
    /// 
    /// For more information on Azure Storage Table Entity, 
    /// see https://docs.azure.cn/zh-cn/dotnet/api/microsoft.windowsazure.storage.table.tableentity
    /// or https://csharp.hotexamples.com/examples/Microsoft.WindowsAzure.Storage.Table/TableEntity/-/php-tableentity-class-examples.html
    /// </summary>
    public class BaseEntity : TableEntity
    {
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}
