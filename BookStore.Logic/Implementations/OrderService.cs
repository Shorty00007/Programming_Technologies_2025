using BookStore.Data.Abstractions;
using BookStore.Data.Models;
using BookStore.Logic.Abstractions;
using Microsoft.EntityFrameworkCore;
namespace BookStore.Logic.Implementations;
public class OrderService : IOrderService
{
    private readonly IBookStoreContext _context;

    public OrderService(IBookStoreContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Book)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersByCustomerIdAsync(int customerId)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .Where(o => o.Status == status)
            .ToListAsync();
    }

    public async Task CreateOrderAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Calculate total amount if not already set
        if (order.TotalAmount == 0 && order.OrderItems.Any())
        {
            order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order != null)
        {
            order.Status = status;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<decimal> CalculateOrderTotalAsync(int orderId)
    {
        var orderItems = await _context.OrderItems
            .Where(oi => oi.OrderId == orderId)
            .ToListAsync();

        return orderItems.Sum(oi => oi.Quantity * oi.UnitPrice);
    }

    public async Task AddItemToOrderAsync(int orderId, int bookId, int quantity)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId);

        var book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == bookId);

        if (order != null && book != null)
        {
            // Check if item already exists in the order
            var existingItem = await _context.OrderItems
                .FirstOrDefaultAsync(oi => oi.OrderId == orderId && oi.BookId == bookId);

            if (existingItem != null)
            {
                // Update quantity if item exists
                existingItem.Quantity += quantity;
            }
            else
            {
                // Add new item
                var orderItem = new OrderItem
                {
                    OrderId = orderId,
                    BookId = bookId,
                    Quantity = quantity,
                    UnitPrice = book.Price
                };

                _context.OrderItems.Add(orderItem);
            }

            await _context.SaveChangesAsync();

            // Update order total
            order.TotalAmount = await CalculateOrderTotalAsync(orderId);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveItemFromOrderAsync(int orderId, int orderItemId)
    {
        var orderItem = await _context.OrderItems
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId && oi.OrderId == orderId);

        if (orderItem != null)
        {
            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();

            // Update order total
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order != null)
            {
                order.TotalAmount = await CalculateOrderTotalAsync(orderId);
                await _context.SaveChangesAsync();
            }
        }
    }

    public async Task<List<OrderItem>> GetOrderItemsAsync(int orderId)
    {
        return await _context.OrderItems
            .Include(oi => oi.Book)
            .Where(oi => oi.OrderId == orderId)
            .ToListAsync();
    }
}