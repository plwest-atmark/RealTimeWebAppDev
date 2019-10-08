// Azure Storage Namespaces
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace AzureStorageTest_Example
{
    class Program
    {
        static void Main()
        {

            Book book;
            //! -------------------------- Part 1 - Basic use of Azure Storage --------------------------

            // Azure Storage Account and Table Service Instances 
            CloudStorageAccount storageAccount;
            CloudTableClient tableClient;

            // Connnect to Storage Account 
            storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            // Create the Table 'Book', if it not exists 
            tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("Book");
            table.CreateIfNotExistsAsync();

            // Create a Book instance 
            book = new Book() { Author = "Rami", BookName = "ASP.NET Core With Azure", Publisher = "APress" };
            book.BookId = 1;
            book.RowKey = book.BookId.ToString();
            book.PartitionKey = book.Publisher;
            book.CreatedDate = DateTime.UtcNow;
            book.UpdatedDate = DateTime.UtcNow;
            // Insert and execute operations 
            TableOperation insertOperation = TableOperation.Insert(book);
            table.ExecuteAsync(insertOperation);

            Console.ReadLine();

            //! -------------------------- Part 2 - Unit of Work Pattern --------------------------

            Task.Run(async () =>
            {
                using (var _unitOfWork = new UnitOfWork("UseDevelopmentStorage=true;"))
                {
                    var bookRepository = _unitOfWork.Repository<Book>();
                    await bookRepository.CreateTableAsync();
                    book = new Book() { Author = "Rami", BookName = "ASP.NET Core With Azure", Publisher = "APress" };
                    book.BookId = 1;
                    book.RowKey = book.BookId.ToString();
                    book.PartitionKey = book.Publisher;

                    var data = await bookRepository.AddAsync(book);
                    Console.WriteLine(data);

                    _unitOfWork.CommitTransaction();
                }

                using (var _unitOfWork = new UnitOfWork("UseDevelopmentStorage=true;"))
                {
                    var bookRepository = _unitOfWork.Repository<Book>();
                    await bookRepository.CreateTableAsync();
                    var data = await bookRepository.FindAsync("APress", "1");
                    Console.WriteLine(data);

                    data.Author = "Rami Vemula";
                    var updatedData = await bookRepository.UpdateAsync(data);
                    Console.WriteLine(updatedData);

                    _unitOfWork.CommitTransaction();
                }

                using (var _unitOfWork = new UnitOfWork("UseDevelopmentStorage=true;"))
                {
                    var bookRepository = _unitOfWork.Repository<Book>();
                    await bookRepository.CreateTableAsync();
                    var data = await bookRepository.FindAsync("APress", "1");
                    Console.WriteLine(data);

                    await bookRepository.DeleteAsync(data);
                    Console.WriteLine("Deleted");

                    // Throw an exception to test rollback actions
                   //  throw new Exception();

                    _unitOfWork.CommitTransaction();
                }
            }).GetAwaiter().GetResult();
        }
    }
}