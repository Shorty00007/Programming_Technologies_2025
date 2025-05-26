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

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        return await _context.Orders
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                UserId = o.UserId,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount
            })
            .ToListAsync();
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

    public async Task<IEnumerable<OrderDto>> GetOrdersForUserAsync(int userId)
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Book)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return orders.Select(o => new OrderDto
        {
            Id = o.Id,
            UserId = o.UserId,
            OrderDate = o.OrderDate,
            TotalAmount = o.TotalAmount,
            Items = o.OrderItems.Select(oi => new OrderItemDto
            {
                BookTitle = oi.Book.Title,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice
            }).ToList()
        });
    }

    public async Task<OrderDto?> GetOrderDetailsAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        return order == null ? null : new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            Username = order.User.Username,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            Items = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                OrderId = oi.OrderId,
                BookId = oi.BookId,
                BookTitle = oi.Book.Title,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice
            }).ToList()
        };
    }
}
