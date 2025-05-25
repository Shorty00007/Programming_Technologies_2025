using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using BookStore.Data.Implementations;
using BookStore.Logic.Implementations;
using BookStore.Logic.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.IntegrationTests
{
    [TestClass]
    public class EventLogServiceIntegrationTests
    {
        private BookStoreContext _context = null!;
        private IBookService _bookService = null!;
        private IEventLogService _eventLogService = null!;
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
            _eventLogService = new EventLogService(_context);
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
        public async Task GetAllLogsAsync_ReturnsLogsAfterBookActions()
        {
            await _bookService.AddBookAsync("Log Test Book", "Logger", "LOG-001", 10m, 1);
            await _bookService.EditBookAsync(1, "Updated Title", "Logger", "LOG-001", 12m, 2);
            await _bookService.RemoveBookAsync(1);

            var logs = await _eventLogService.GetAllLogsAsync();

            Assert.IsTrue(logs.Count() >= 3);
            Assert.IsTrue(logs.Any(l => l.EventType == "Book Added"));
            Assert.IsTrue(logs.Any(l => l.EventType == "Book Edited"));
            Assert.IsTrue(logs.Any(l => l.EventType == "Book Removed"));
        }
    }
}
