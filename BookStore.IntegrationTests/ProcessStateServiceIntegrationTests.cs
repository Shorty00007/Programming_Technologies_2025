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
    public class ProcessStateServiceIntegrationTests
    {
        private BookStoreContext _context = null!;
        private IBookService _bookService = null!;
        private IProcessStateService _processStateService = null!;
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

            _bookService = new BookService(_context); // This internally updates ProcessState
            _processStateService = new ProcessStateService(_context);
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
        public async Task GetLatestSnapshotAsync_ReturnsMostRecent()
        {
            await _bookService.AddBookAsync("Snapshot Test", "Tester", "SNAP123", 10m, 5);

            var latest = await _processStateService.GetLatestSnapshotAsync();

            Assert.IsNotNull(latest);
            Assert.IsTrue(latest!.TotalBooksInStock >= 5);
        }

        [TestMethod]
        [DoNotParallelize]
        public async Task GetSnapshotByDateAsync_ReturnsSnapshotForToday()
        {
            await _bookService.AddBookAsync("Today Book", "Author", "TODAY1", 5m, 3);

            var today = DateTime.UtcNow;
            var snapshot = await _processStateService.GetSnapshotByDateAsync(today);

            Assert.IsNotNull(snapshot);
            Assert.AreEqual(today.Date, snapshot!.RecordedAt.Date);
        }

        [TestMethod]
        [DoNotParallelize]
        public async Task GetSnapshotByDateAsync_ReturnsNullForFutureDate()
        {
            var futureDate = DateTime.UtcNow.AddDays(10);

            var snapshot = await _processStateService.GetSnapshotByDateAsync(futureDate);

            Assert.IsNull(snapshot);
        }
    }
}
