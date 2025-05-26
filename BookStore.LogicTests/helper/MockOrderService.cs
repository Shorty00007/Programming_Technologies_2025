using BookStore.Contracts;
using BookStore.Data.Models;
using BookStore.Logic.Abstractions;

namespace BookStore.LogicTests.helper;
public class MockOrderService : IOrderService
{
    private readonly List<Book> _books;
    private readonly List<User> _users;
    private readonly List<Order> _orders = new();
    private readonly List<EventLog> _logs = new();
    private readonly List<ProcessState> _snapshots = new();
    private int _orderId = 1;

    public MockOrderService(List<User> users, List<Book> books)
    {
        _users = users;
        _books = books;
    }

    public Task<OrderDto> PlaceOrderAsync(int userId, List<(int bookId, int quantity)> items)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user == null) throw new InvalidOperationException("User not found.");

        var order = new Order
        {
            Id = _orderId++,
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 0m,
            OrderItems = new List<OrderItem>()
        };

        foreach (var (bookId, quantity) in items)
        {
            var book = _books.FirstOrDefault(b => b.Id == bookId);
            if (book == null) throw new InvalidOperationException($"Book {bookId} not found.");
            if (book.Stock < quantity) throw new InvalidOperationException($"Not enough stock for {book.Title}");

            book.Stock -= quantity;

            order.OrderItems.Add(new OrderItem
            {
                BookId = book.Id,
                Quantity = quantity,
                UnitPrice = book.Price
            });

            order.TotalAmount += book.Price * quantity;
        }

        _orders.Add(order);

        LogEvent("Order Placed", $"User {user.Username} placed an order #{order.Id}", userId);
        UpdateProcessState();

        return Task.FromResult(new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount
        });
    }

    private void LogEvent(string type, string desc, int? userId = null)
    {
        _logs.Add(new EventLog
        {
            EventType = type,
            Description = desc,
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });
    }

    private void UpdateProcessState()
    {
        var totalBooksInStock = _books.Sum(b => b.Stock);
        var totalOrders = _orders.Count;
        var totalRevenue = _orders.Sum(o => o.TotalAmount);

        _snapshots.Add(new ProcessState
        {
            TotalBooksInStock = totalBooksInStock,
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue
        });
    }

    // Pomocnicze metody do testów
    public IEnumerable<EventLog> GetLogs() => _logs;
    public IEnumerable<ProcessState> GetSnapshots() => _snapshots;
    public IEnumerable<Order> GetOrders() => _orders;

    public Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<OrderDto>> GetOrdersForUserAsync(int userId)
    {
        throw new NotImplementedException();
    }
}
