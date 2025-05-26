// BookStore.PresentationTests/ViewModels/CustomerDashboardViewModelTests.cs

using BookStore.Contracts;
using BookStore.Logic.Abstractions;
using BookStore.Presentation.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System;

namespace BookStore.PresentationTests.ViewModels
{
    [TestClass]
    public class CustomerDashboardViewModelTests
    {
        private class FakeBookService : IBookService
        {
            public List<BookDto> Books { get; set; } = new List<BookDto>();

            public Task<IEnumerable<BookDto>> GetAllBooksAsync() => Task.FromResult(Books.AsEnumerable());

            public Task<BookDto> AddBookAsync(string title, string author, string isbn, decimal price, int stock) => Task.FromResult(new BookDto());
            public Task<bool> RemoveBookAsync(int id) => Task.FromResult(true);
            public Task<BookDto?> EditBookAsync(int id, string title, string author, string isbn, decimal price, int stock) => Task.FromResult<BookDto?>(null);
            public Task<IEnumerable<BookDto>> GetBooksByAuthorAsync(string author) => Task.FromResult<IEnumerable<BookDto>>(new List<BookDto>());
            public Task<BookDto?> GetBookByIsbnAsync(string isbn) => Task.FromResult<BookDto?>(null);
            public Task UpdateProcessStateAsync() => Task.CompletedTask;
        }

        private class FakeOrderService : IOrderService
        {
            public List<(int userId, List<(int bookId, int quantity)> items)> Orders = new();

            public Task<OrderDto> PlaceOrderAsync(int userId, List<(int bookId, int quantity)> items)
            {
                Orders.Add((userId, items));
                return Task.FromResult(new OrderDto { Id = 1, UserId = userId });
            }

            public Task<IEnumerable<OrderDto>> GetOrdersForUserAsync(int userId)
            {
                return Task.FromResult<IEnumerable<OrderDto>>(new List<OrderDto>());
            }

            public Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
            {
                return Task.FromResult<IEnumerable<OrderDto>>(new List<OrderDto>());
            }
        }

        private class TestCustomerDashboardViewModel : CustomerDashboardViewModel
        {
            public TestCustomerDashboardViewModel(IBookService bookService, IOrderService orderService, UserDto currentUser)
                : base(bookService, orderService, currentUser)
            {
            }

            protected virtual void ShowMessage(string message) { }

        }

        [TestMethod]
        public async Task LoadDataAsync_ShouldOnlyLoadBooksWithStock()
        {
            // Arrange
            var bookService = new FakeBookService
            {
                Books = new List<BookDto>
                {
                    new BookDto { Id = 1, Title = "Book A", Stock = 3 },
                    new BookDto { Id = 2, Title = "Book B", Stock = 0 },
                    new BookDto { Id = 3, Title = "Book C", Stock = 1 }
                }
            };

            var orderService = new FakeOrderService();
            var user = new UserDto { Id = 42 };

            var vm = new TestCustomerDashboardViewModel(bookService, orderService, user);

            await Task.Delay(100);

            // Act
            var loadedTitles = vm.AvailableBooks.Select(b => b.Title).ToList();

            // Assert
            Assert.IsTrue(loadedTitles.Contains("Book A"));
            Assert.IsTrue(loadedTitles.Contains("Book C"));
            Assert.IsFalse(loadedTitles.Contains("Book B"));
        }

        [TestMethod]
        public void OrderSelectedBookAsync_ShouldPlaceCorrectOrder()
        {
            // Arrange
            var bookA = new BookDto { Id = 1, Title = "A", Stock = 3 };
            var bookB = new BookDto { Id = 2, Title = "B", Stock = 2 };

            var bookService = new FakeBookService { Books = new List<BookDto> { bookA, bookB } };
            var orderService = new FakeOrderService();
            var user = new UserDto { Id = 10 };

            var vm = new TestCustomerDashboardViewModel(bookService, orderService, user);
            Task.Delay(100).Wait();

            vm.SelectedBooks.Add(bookA);
            vm.SelectedBooks.Add(bookB);

            // Act
            ICommand command = vm.OrderSelectedBookCommand;
            command.Execute(null);

            // Assert
            Assert.AreEqual(1, orderService.Orders.Count);
            var (userId, items) = orderService.Orders.First();
            Assert.AreEqual(10, userId);
            Assert.AreEqual(2, items.Count);
            Assert.IsTrue(items.Any(i => i.bookId == 1));
            Assert.IsTrue(items.Any(i => i.bookId == 2));
        }
    }
}