using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using BookStore.Data;
using BookStore.Data.Models;
using System;
using System.Linq;
using BookStore.Data.Implementations;

namespace BookStore.DataTests
{
    [TestClass]
    public class DataLayerTests
    {
        private BookStoreContext _context = null!;
        private string _connectionString = null!;
        private string _dbName = null!;

        [TestInitialize]
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
        }

        [TestCleanup]
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
        public void AddUser()
        {
            var user = new User
            {
                Username = "testuser",
                PasswordHash = "hashed123",
                Role = "Customer"
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            var saved = _context.Users.FirstOrDefault(u => u.Username == "testuser");

            Assert.IsNotNull(saved);
            Assert.AreEqual("Customer", saved!.Role);
        }

        [TestMethod]
        [DoNotParallelize]
        public void AddBook()
        {
            var book = new Book
            {
                Title = "Book A",
                Author = "Author A",
                ISBN = "1234567890",
                Price = 10.99m,
                Stock = 5
            };

            _context.Books.Add(book);
            _context.SaveChanges();

            var saved = _context.Books.FirstOrDefault(b => b.ISBN == "1234567890");

            Assert.IsNotNull(saved);
            Assert.AreEqual("Book A", saved!.Title);
        }

        [TestMethod]
        [DoNotParallelize]
        public void AddOrder()
        {
            var user = new User
            {
                Username = "orderuser",
                PasswordHash = "pass",
                Role = "Customer"
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 50.00m
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            var saved = _context.Orders.FirstOrDefault(o => o.UserId == user.Id);

            Assert.IsNotNull(saved);
            Assert.AreEqual(50.00m, saved!.TotalAmount);
        }

        [TestMethod]
        [DoNotParallelize]
        public void AddOrderItem()
        {
            var book = new Book
            {
                Title = "Book for OrderItem",
                Author = "Author X",
                ISBN = "999888777",
                Price = 15.00m,
                Stock = 10
            };
            _context.Books.Add(book);

            var user = new User
            {
                Username = "orderitemuser",
                PasswordHash = "secure",
                Role = "Customer"
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 15.00m
            };
            _context.Orders.Add(order);
            _context.SaveChanges();

            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                BookId = book.Id,
                Quantity = 1,
                UnitPrice = 15.00m
            };

            _context.OrderItems.Add(orderItem);
            _context.SaveChanges();

            var saved = _context.OrderItems.FirstOrDefault(oi => oi.OrderId == order.Id);

            Assert.IsNotNull(saved);
            Assert.AreEqual(book.Id, saved!.BookId);
        }

        [TestMethod]
        [DoNotParallelize]
        public void AddProcessState()
        {
            var state = new ProcessState
            {
                TotalBooksInStock = 100,
                TotalOrders = 25,
                TotalRevenue = 1200.50m
            };

            _context.ProcessStates.Add(state);
            _context.SaveChanges();

            var saved = _context.ProcessStates.FirstOrDefault();

            Assert.IsNotNull(saved);
            Assert.AreEqual(100, saved!.TotalBooksInStock);
        }

        [TestMethod]
        [DoNotParallelize]
        public void AddEventLog()
        {
            var user = new User
            {
                Username = "eventuser",
                PasswordHash = "xyz",
                Role = "Admin"
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            var log = new EventLog
            {
                EventType = "Login",
                Description = "User logged in",
                UserId = user.Id
            };

            _context.EventLogs.Add(log);
            _context.SaveChanges();

            var saved = _context.EventLogs.FirstOrDefault(e => e.UserId == user.Id);

            Assert.IsNotNull(saved);
            Assert.AreEqual("Login", saved!.EventType);
        }
    }
}
