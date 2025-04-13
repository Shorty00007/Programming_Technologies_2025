using BookStore.Data.Models;
namespace BookStore.Logic.Abstractions;
public interface IOrderItemService
{
    Task<OrderItem?> GetOrderItemByIdAsync(int id);
    Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId);
    Task<List<OrderItem>> GetOrderItemsByBookIdAsync(int bookId);
    Task UpdateOrderItemQuantityAsync(int orderItemId, int quantity);
    Task RemoveOrderItemAsync(int orderItemId);
}