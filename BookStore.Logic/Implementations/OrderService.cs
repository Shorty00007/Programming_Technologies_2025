using BookStore.Contracts;
using BookStore.Data.Abstractions;
using BookStore.Data.Models;
using BookStore.Logic.Abstractions;
using Microsoft.EntityFrameworkCore;

public class OrderService : IOrderService
{
    private readonly IBookStoreContext _context;

    public OrderService(IBookStoreContext context)
    {
        _context = context;
    }

    public async Task<OrderDto> PlaceOrderAsync(int userId, List<(int bookId, int quantity)> items)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new InvalidOperationException("User not found.");

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 0m
        };

        foreach (var (bookId, quantity) in items)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null) throw new InvalidOperationException($"Book {bookId} not found.");
            if (book.Stock < quantity) throw new InvalidOperationException($"Not enough stock for {book.Title}");

            var orderItem = new OrderItem
            {
                BookId = book.Id,
                Quantity = quantity,
                UnitPrice = book.Price
            };

            order.OrderItems.Add(orderItem);
            book.Stock -= quantity;
            order.TotalAmount += book.Price * quantity;
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        await LogEventAsync("Order Placed", $"User {user.Username} placed an order #{order.Id}", userId);
        await UpdateProcessStateAsync();

        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount
        };
    }

    private async Task LogEventAsync(string eventType, string description, int? userId = null)
    {
        var log = new EventLog
        {
            EventType = eventType,
            Description = description,
            UserId = userId
        };

        _context.EventLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    private async Task UpdateProcessStateAsync()
    {
        var totalBooksInStock = await _context.Books.SumAsync(b => b.Stock);
        var totalOrders = await _context.Orders.CountAsync();
        var totalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount);

        var snapshot = new ProcessState
        {
            TotalBooksInStock = totalBooksInStock,
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue
        };

        _context.ProcessStates.Add(snapshot);
        await _context.SaveChangesAsync();
    }
}
