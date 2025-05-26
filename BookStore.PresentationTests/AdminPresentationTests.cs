using BookStore.Contracts;
using BookStore.Logic.Abstractions;
using BookStore.Presentation.ViewModels.Admin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.PresentationTests.ViewModels
{
    [TestClass]
    public class AdminPresentationTests
    {
        private class FakeBookService : IBookService
        {
            public List<BookDto> Books = new()
            {
                new BookDto { Id = 1, Title = "Book A", Author = "Author A", ISBN = "1234567890", Price = 9.99m, Stock = 10 },
                new BookDto { Id = 2, Title = "Book B", Author = "Author B", ISBN = "0987654321", Price = 19.99m, Stock = 5 }
            };

            public Task<IEnumerable<BookDto>> GetAllBooksAsync() => Task.FromResult(Books.AsEnumerable());
            public Task<BookDto> AddBookAsync(string title, string author, string isbn, decimal price, int stock)
            {
                var book = new BookDto { Id = Books.Count + 1, Title = title, Author = author, ISBN = isbn, Price = price, Stock = stock };
                Books.Add(book);
                return Task.FromResult(book);
            }

            public Task<bool> RemoveBookAsync(int id)
            {
                var b = Books.FirstOrDefault(x => x.Id == id);
                if (b != null) Books.Remove(b);
                return Task.FromResult(true);
            }

            public Task<BookDto?> EditBookAsync(int id, string title, string author, string isbn, decimal price, int stock)
            {
                var book = Books.FirstOrDefault(b => b.Id == id);
                if (book != null)
                {
                    book.Title = title;
                    book.Author = author;
                    book.ISBN = isbn;
                    book.Price = price;
                    book.Stock = stock;
                }
                return Task.FromResult<BookDto?>(book);
            }

            public Task<IEnumerable<BookDto>> GetBooksByAuthorAsync(string author) =>
                Task.FromResult(Books.Where(b => b.Author == author).AsEnumerable());

            public Task<BookDto?> GetBookByIsbnAsync(string isbn) =>
                Task.FromResult<BookDto?>(Books.FirstOrDefault(b => b.ISBN == isbn));

            public Task UpdateProcessStateAsync() => Task.CompletedTask;
        }

        private class FakeEventLogService : IEventLogService
        {
            public List<EventLogDto> Logs = new()
            {
                new EventLogDto { Id = 1, EventType = "Book Added", Description = "Added Book A", Timestamp = DateTime.UtcNow },
                new EventLogDto { Id = 2, EventType = "Order Placed", Description = "User placed an order", Timestamp = DateTime.UtcNow }
            };

            public Task<IEnumerable<EventLogDto>> GetAllLogsAsync() => Task.FromResult(Logs.AsEnumerable());
        }

        private class FakeProcessStateService : IProcessStateService
        {
            public DateTime LastRequestedDate { get; private set; }

            public Task<ProcessStateDto?> GetLatestSnapshotAsync() =>
                Task.FromResult<ProcessStateDto?>(new ProcessStateDto
                {
                    RecordedAt = DateTime.UtcNow,
                    TotalBooksInStock = 15,
                    TotalOrders = 3,
                    TotalRevenue = 99.99m
                });

            public Task<ProcessStateDto?> GetSnapshotByDateAsync(DateTime date)
            {
                LastRequestedDate = date;
                return Task.FromResult<ProcessStateDto?>(new ProcessStateDto
                {
                    RecordedAt = date,
                    TotalBooksInStock = 10,
                    TotalOrders = 2,
                    TotalRevenue = 50.00m
                });
            }
        }


        private class FakeOrderService : IOrderService
        {
            public Task<IEnumerable<OrderDto>> GetAllOrdersAsync() =>
                Task.FromResult<IEnumerable<OrderDto>>(new[]
                {
                    new OrderDto
                    {
                        Id = 1,
                        UserId = 5,
                        OrderDate = DateTime.UtcNow,
                        TotalAmount = 25.00m,
                        Items = new List<OrderItemDto>
                        {
                            new OrderItemDto { BookTitle = "Book A", Quantity = 1, UnitPrice = 25.00m }
                        }
                    }
                });

            public Task<IEnumerable<OrderDto>> GetOrdersForUserAsync(int userId) => Task.FromResult<IEnumerable<OrderDto>>(Array.Empty<OrderDto>());
            public Task<OrderDto?> GetOrderDetailsAsync(int orderId) => Task.FromResult<OrderDto?>(null);
            public Task<OrderDto> PlaceOrderAsync(int userId, List<(int bookId, int quantity)> items) => Task.FromResult(new OrderDto());
        }

        [TestMethod]
        [DoNotParallelize]
        public async Task AdminPanels_ShouldLoadDataCorrectly()
        {
            var bookService = new FakeBookService();
            var eventService = new FakeEventLogService();
            var processService = new FakeProcessStateService();
            var orderService = new FakeOrderService();

            var booksVm = new BooksPanelViewModel(bookService);
            await Task.Delay(100); // let LoadAsync run
            Assert.AreEqual(bookService.Books.Count, booksVm.Books.Count);
            CollectionAssert.AreEqual(bookService.Books, booksVm.Books.ToList());

            var logsVm = new LogsViewModel(eventService)
            {
                FilterText = "Book"
            };
            logsVm.FilterCommand.Execute(null);
            Assert.IsTrue(logsVm.Logs.All(l => l.EventType.Contains("Book")));

            var psVm = new ProcessStateViewModel(processService);
            await Task.Delay(100);
            Assert.IsTrue(psVm.SnapshotText.Contains("Total Orders"));

            var ordersVm = new OrdersPanelViewModel(orderService);
            await Task.Delay(100);
            Assert.AreEqual(1, ordersVm.Orders.Count);
            Assert.AreEqual("Book A", ordersVm.Orders.First().Items.First().BookTitle);
        }


        [TestMethod]
        [DoNotParallelize]
        public void AddBook_ShouldAddToList()
        {
            var bookService = new FakeBookService();
            var vm = new BooksPanelViewModel(bookService);

            vm.Form.Title = "New Book";
            vm.Form.Author = "Tester";
            vm.Form.ISBN = "1112223334";
            vm.PriceText = "15.50";
            vm.Form.Stock = 7;

            vm.AddCommand.Execute(null);

            Assert.IsTrue(vm.Books.Any(b => b.Title == "New Book"));
            Assert.IsTrue(bookService.Books.Any(b => b.Title == "New Book"));
        }

        [TestMethod]
        [DoNotParallelize]
        public async Task SaveBook_ShouldUpdateBook()
        {
            var bookService = new FakeBookService();
            var vm = new BooksPanelViewModel(bookService);
            await Task.Delay(100);

            var original = vm.Books.First();
            vm.SelectedBook = original;

            vm.Form.Title = "Updated Title";
            vm.Form.Author = original.Author;
            vm.Form.ISBN = original.ISBN;
            vm.PriceText = original.Price.ToString(System.Globalization.CultureInfo.InvariantCulture);
            vm.Form.Stock = original.Stock;

            vm.SaveCommand.Execute(null);

            var updated = bookService.Books.First(b => b.Id == original.Id);
            Assert.AreEqual("Updated Title", updated.Title);
        }


        [TestMethod]
        [DoNotParallelize]
        public async Task DeleteBook_ShouldRemoveBook()
        {
            var bookService = new FakeBookService();
            var vm = new BooksPanelViewModel(bookService);
            await Task.Delay(100);

            var toRemove = vm.Books.First();
            vm.SelectedBook = toRemove;

            vm.DeleteCommand.Execute(null);

            Assert.IsFalse(bookService.Books.Any(b => b.Id == toRemove.Id));
        }


        [TestMethod]
        [DoNotParallelize]
        public void Logs_FilterCommand_ShouldReturnMatching()
        {
            var logService = new FakeEventLogService();
            var vm = new LogsViewModel(logService)
            {
                FilterText = "Order"
            };

            vm.FilterCommand.Execute(null);

            Assert.IsTrue(vm.Logs.All(l => l.EventType.Contains("Order")));
            Assert.IsTrue(vm.Logs.Count <= logService.Logs.Count);
        }


        [TestMethod]
        [DoNotParallelize]
        public async Task ProcessState_LoadByDate_ShouldShowCorrectInfo()
        {
            var psService = new FakeProcessStateService();
            var testDate = new DateTime(2024, 5, 15);
            var vm = new ProcessStateViewModel(psService)
            {
                SelectedDate = testDate
            };

            vm.LoadByDateCommand.Execute(null);
            await Task.Delay(100);

            Assert.IsTrue(vm.SnapshotText.Contains("Total Orders"));
            Assert.AreEqual(testDate, psService.LastRequestedDate);
        }
    }
}
