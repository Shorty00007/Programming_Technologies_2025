using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using BookStore.Data.Implementations;
using BookStore.Logic.Abstractions;
using BookStore.Logic.Implementations;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.IntegrationTests
{
    [TestClass]
    public class BookServiceIntegrationTests
    {
        private BookStoreContext _context = null!;
        private IBookService _bookService = null!;
        private string _connectionString = null!;
        private string _dbName = null!;

        [TestInitialize]
        [DoNotParallelize]
        public void Init()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            _connectionString = config.GetConnectionString("DefaultConnection")!;
            _dbName = new SqlConnectionStringBuilder(_connectionString).InitialCatalog!;

            var options = new DbContextOptionsBuilder<BookStoreContext>()
                .UseSqlServer(_connectionString)
                .Options;

            _context = new BookStoreContext(options);
            _context.Database.EnsureCreated();

            _bookService = new BookService(_context);
        }

        [TestCleanup]
        [DoNotParallelize]
        public void Cleanup()
        {
            var masterConnection = _connectionString.Replace(_dbName, "master");

            using var conn = new SqlConnection(masterConnection);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
                IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{_dbName}')
                BEGIN
                    ALTER DATABASE [{_dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [{_dbName}];
                END";

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cleanup failed: {ex.Message}");
            }
        }

        [TestMethod]
        [DoNotParallelize]
        public async Task CanAddAndGetBook()
        {
            var added = await _bookService.AddBookAsync("Test Title", "Test Author", "ISBN123", 15.99m, 5);

            Assert.IsNotNull(added);
            Assert.AreEqual("Test Title", added.Title);

            var byIsbn = await _bookService.GetBookByIsbnAsync("ISBN123");
            Assert.IsNotNull(byIsbn);
            Assert.AreEqual("Test Author", byIsbn!.Author);
        }

        [TestMethod]
        [DoNotParallelize]
        public async Task CanEditBook()
        {
            var added = await _bookService.AddBookAsync("Old Title", "Author", "Edit123", 10.0m, 3);

            var edited = await _bookService.EditBookAsync(added.Id, "New Title", "New Author", "Edit123", 12.5m, 7);

            Assert.IsNotNull(edited);
            Assert.AreEqual("New Title", edited!.Title);
            Assert.AreEqual(12.5m, edited.Price);
        }

        [TestMethod]
        [DoNotParallelize]
        public async Task CanRemoveBook()
        {
            var added = await _bookService.AddBookAsync("To Delete", "Author", "Delete123", 9.99m, 2);

            var removed = await _bookService.RemoveBookAsync(added.Id);
            var found = await _bookService.GetBookByIsbnAsync("Delete123");

            Assert.IsTrue(removed);
            Assert.IsNull(found);
        }

        [TestMethod]
        [DoNotParallelize]
        public async Task GetAllBooks_ReturnsAll()
        {
            await _bookService.AddBookAsync("Book 1", "A", "ALL1", 5m, 1);
            await _bookService.AddBookAsync("Book 2", "B", "ALL2", 10m, 2);

            var all = await _bookService.GetAllBooksAsync();

            Assert.AreEqual(2, all.Count());
        }

        [TestMethod]
        [DoNotParallelize]
        public async Task GetBooksByAuthor_ReturnsCorrectBooks()
        {
            await _bookService.AddBookAsync("Book by X", "XAuthor", "X1", 7m, 1);
            await _bookService.AddBookAsync("Another by X", "XAuthor", "X2", 8m, 2);
            await _bookService.AddBookAsync("Not by X", "Other", "X3", 8m, 2);

            var results = await _bookService.GetBooksByAuthorAsync("xauthor");

            Assert.AreEqual(2, results.Count());
            Assert.IsTrue(results.All(b => b.Author == "XAuthor"));
        }
    }
}
