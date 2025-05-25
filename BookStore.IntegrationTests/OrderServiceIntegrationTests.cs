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
using System.Collections.Generic;

namespace BookStore.IntegrationTests
{
    [TestClass]
    public class OrderServiceIntegrationTests
    {
        private BookStoreContext _context = null!;
        private IUserService _userService = null!;
        private IBookService _bookService = null!;
        private IOrderService _orderService = null!;
        private IProcessStateService _processStateService = null!;
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

            _userService = new UserService(_context);
            _bookService = new BookService(_context);
            _orderService = new OrderService(_context);
            _processStateService = new ProcessStateService(_context);
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
        public async Task CanPlaceOrder_ReducesStock_CreatesSnapshot()
        {
            var user = await _userService.Register("orderuser", "pass");
            var book = await _bookService.AddBookAsync("Test Book", "Author", "ORDER123", 20m, 10);

            var items = new List<(int bookId, int quantity)>
            {
                (book.Id, 2)
            };

            var order = await _orderService.PlaceOrderAsync(user.Id, items);

            Assert.IsNotNull(order);
            Assert.AreEqual(user.Id, order.UserId);
            Assert.AreEqual(40m, order.TotalAmount);

            var updatedBook = await _bookService.GetBookByIsbnAsync("ORDER123");
            Assert.AreEqual(8, updatedBook!.Stock);

            var snapshot = await _processStateService.GetLatestSnapshotAsync();
            Assert.IsNotNull(snapshot);
            Assert.AreEqual(8, snapshot!.TotalBooksInStock);
            Assert.AreEqual(1, snapshot.TotalOrders);
            Assert.AreEqual(40m, snapshot.TotalRevenue);

            var logs = await _eventLogService.GetAllLogsAsync();
            var log = logs.FirstOrDefault();

            Assert.IsNotNull(log);
            Assert.AreEqual("Order Placed", log!.EventType);
            Assert.IsTrue(log.Description.Contains($"order #{order.Id}"));
        }
    }
}
