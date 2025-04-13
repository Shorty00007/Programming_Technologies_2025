using BookStore.Data.Abstractions;
using BookStore.Data.Models;
using BookStore.Logic.Abstractions;
using Microsoft.EntityFrameworkCore;
namespace BookStore.Logic.Implementations;
public class OrderItemService : IOrderItemService
{
    private readonly IBookStoreContext _context;

    public OrderItemService(IBookStoreContext context)
    {
        _context = context;
    }

    public async Task<OrderItem?> GetOrderItemByIdAsync(int id)
    {
        return await _context.OrderItems
            .Include(oi => oi.Book)
            .Include(oi => oi.Order)
            .FirstOrDefaultAsync(oi => oi.Id == id);
    }

    public async Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId)
    {
        return await _context.OrderItems
            .Include(oi => oi.Book)
            .Where(oi => oi.OrderId == orderId)
            .ToListAsync();
    }

    public async Task<List<OrderItem>> GetOrderItemsByBookIdAsync(int bookId)
    {
        return await _context.OrderItems
            .Include(oi => oi.Order)
            .Where(oi => oi.BookId == bookId)
            .ToListAsync();
    }

    public async Task UpdateOrderItemQuantityAsync(int orderItemId, int quantity)
    {
        var orderItem = await _context.OrderItems
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId);

        if (orderItem != null)
        {
            orderItem.Quantity = quantity;
            await _context.SaveChangesAsync();

            // Update the order total
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderItem.OrderId);

            if (order != null)
            {
                order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);
                await _context.SaveChangesAsync();
            }
        }
    }

    public async Task RemoveOrderItemAsync(int orderItemId)
    {
        var orderItem = await _context.OrderItems
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId);

        if (orderItem != null)
        {
            int orderId = orderItem.OrderId;

            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();

            // Update the order total
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order != null)
            {
                order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);
                await _context.SaveChangesAsync();
            }
        }
    }
}