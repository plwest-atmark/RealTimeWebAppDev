// Azure Storage Namespaces
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading;
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


            ShowThreadInfo("Application"); //! used to display the "tread" that this "application" is running on.

            // Queues the specified work to run on the ThreadPool and returns a task or Task<TResult> handle for that work.
            // Basically, this means that this "block" of code will create it's own thread to run on seperate from
            // our application thread.  This is a means to handle simple multithreading so our application can remain
            // responsive when things are happening in the background. 
            Task.Run(async () =>
            {
                ShowThreadInfo("Task"); //! used to display the "tread" that this "task" is running on.

                //? Using the Unit of Work design pattern is quite simple.  You create a "unit of work" object
                //? passing the connection string for the database that you want to connect to. After that,
                //? you use the "_unitOfWork" class either create new or retrieve existing repositories.
                //? When we first use "_unitOfWork.Repository<Book>();", the "Book" repository will be created
                //? however, after this it will not need to be created again. This is called "lazy initialization"
                //? This is an effective means to manage both CPU usage and memory. If we do not need to create
                //? something we will not create it. However, we will create it at the first time we need it, and
                //? re-use the same object after creation if we can.
                using (var _unitOfWork = new UnitOfWork("UseDevelopmentStorage=true;"))
                {
                    // create the "Book" repository.
                    var bookRepository = _unitOfWork.Repository<Book>();
                    //  create the table in the Azure Storage Data Source. ONLY IF IT'S NOT ALREADY CREATED.
                    //! see the Repository class to verify that it will only be created if it doesn't exist.
                    await bookRepository.CreateTableAsync();

                    // create a new book
                    //! NOTE: The book class will handle setting the PartitionKey and RowKey for the book. This means
                    //!       that the consumer of the book does not have to worry about how this is managed. Also,
                    //!       this will ensure that the PartionKey and RowKey are set automatically.
                    book = new Book() { Author = "Rami", BookName = "ASP.NET Core With Azure", Publisher = "APress" };

                    // using the repository, we will "Add" the book to the Azure Table
                    var data = await bookRepository.AddAsync(book);
                    // print the data for our own record.
                    Console.WriteLine(data);


                    // ensure that the "entire" transaction is committed.  Since, we only did one action, this isn't
                    // very sever. However, there may be times when we add, update and delete many things in one transaction
                    // this is why we will use the Unit of Work. If anything fails, we want everything to fail to ensure
                    // data integrity.
                    _unitOfWork.CommitTransaction();
                }

                using (var _unitOfWork = new UnitOfWork("UseDevelopmentStorage=true;"))
                {
                    // since this is a new Unit of Work, we need to again get the book repository
                    var bookRepository = _unitOfWork.Repository<Book>();
                    // since the consumer does not know if there is a table that exists for this repository
                    // we will attempt to create it.
                    //! NOTE: If the table already exists, it will not be created again.
                    await bookRepository.CreateTableAsync();

                    // "Find" the book based on the PartitionKey and RowKey
                    var data = await bookRepository.FindAsync("APress", "1");
                    // print the data for our own record.
                    Console.WriteLine(data);

                    // Note that in this transaction we are also doing an update. This will be done after the find,
                    // but the transaction will be one "Unit of Work" and will not be done until we "commit" the transaction
                    // at the end of this Unit of Work.
                    data.Author = "Rami Vemula";
                    // Update the book that we got with the find
                    var updatedData = await bookRepository.UpdateAsync(data);
                    // print the data for our own record.
                    Console.WriteLine(updatedData);


                    // Commit the transaction.
                    //! NOTE: both the find and update will occur at this point. So, any changes to the database have
                    //!       not been finalized until this point in the program code.
                    _unitOfWork.CommitTransaction();
                }

                using (var _unitOfWork = new UnitOfWork("UseDevelopmentStorage=true;"))
                {
                    var bookRepository = _unitOfWork.Repository<Book>();
                    await bookRepository.CreateTableAsync();
                    var data = await bookRepository.FindAsync("APress", "1");
                    Console.WriteLine(data);

                    await bookRepository.DeleteAsync(data);
                    Console.WriteLine($"Deleted: {data}");

                    // Throw an exception to test rollback actions
                    //  throw new Exception();

                    _unitOfWork.CommitTransaction();
                }
            }).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Method to show thread information based on tasks.
        /// </summary>
        /// <param name="s"></param>
        static void ShowThreadInfo(String s)
        {
            Console.WriteLine("{0} Thread ID: {1}",
                              s, Thread.CurrentThread.ManagedThreadId);
        }
    }
}