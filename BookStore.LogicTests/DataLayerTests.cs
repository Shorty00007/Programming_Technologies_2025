using Microsoft.VisualStudio.TestTools.UnitTesting;
using BookStore.Contracts;
using BookStore.Logic.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using BookStore.LogicTests.helper;

namespace BookStore.LogicTests
{
    [TestClass]
    public class LogicLayerTests
    {
        [TestMethod]
        public void AddBook_ShouldReturnBookDto()
        {
            var service = new MockBookService();

            var result = service.AddBookAsync("Test Title", "Author X", "ISBN123", 99.99m, 10).Result;

            Assert.IsNotNull(result);
            Assert.AreEqual("Test Title", result.Title);
            Assert.AreEqual(10, result.Stock);
        }

        [TestMethod]
        public void GetAllLogs_ShouldReturnCorrectOrder()
        {
            var service = new MockEventLogService();
            service.AddLog("Event1", "Description1");
            service.AddLog("Event2", "Description2");

            var logs = service.GetAllLogsAsync().Result.ToList();

            Assert.AreEqual(2, logs.Count);
            Assert.IsTrue(logs[0].Timestamp >= logs[1].Timestamp); // descending
        }

        [TestMethod]
        public void PlaceOrder_ShouldCalculateTotalAmount()
        {
            var users = new List<BookStore.Data.Models.User>
            {
                new BookStore.Data.Models.User { Id = 1, Username = "testuser" }
            };

            var books = new List<BookStore.Data.Models.Book>
            {
                new BookStore.Data.Models.Book { Id = 1, Title = "Book A", Price = 10m, Stock = 5 }
            };

            var service = new MockOrderService(users, books);

            var result = service.PlaceOrderAsync(1, new List<(int bookId, int quantity)>
            {
                (1, 2)
            }).Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(20m, result.TotalAmount);
        }

        [TestMethod]
        public void GetLatestSnapshot_ShouldReturnMostRecent()
        {
            var service = new MockProcessStateService();
            service.AddSnapshot(new ProcessStateDto { RecordedAt = DateTime.UtcNow.AddHours(-1), TotalOrders = 1 });
            service.AddSnapshot(new ProcessStateDto { RecordedAt = DateTime.UtcNow, TotalOrders = 2 });

            var latest = service.GetLatestSnapshotAsync().Result;

            Assert.IsNotNull(latest);
            Assert.AreEqual(2, latest!.TotalOrders);
        }

        [TestMethod]
        public void RegisterAndLogin_ShouldWork()
        {
            var service = new MockUserService();

            var registered = service.Register("newuser", "pass").Result;
            var loggedIn = service.Login("newuser", "pass");

            Assert.IsNotNull(loggedIn);
            Assert.AreEqual(registered.Username, loggedIn!.Username);
        }
    }
}
