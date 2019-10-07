// Azure Storage Namespaces
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureStorageTest
{
    class Program
    {
        static void Main()
        {
            Task.Run(async () =>
            {
                using (var _unitOfWork = new UnitOfWork("UseDevelopmentStorage=true;"))
                {
                    var bookRepository = _unitOfWork.Repository<Book>();
                    await bookRepository.CreateTableAsync();
                    Book book = new Book() { Author = "Rami", BookName = "ASP.NET Core With Azure", Publisher = "APress" };
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

    public class BaseEntity : TableEntity
    {
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }

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

        public int BookId { get; set; }
        public string BookName { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
    }
}