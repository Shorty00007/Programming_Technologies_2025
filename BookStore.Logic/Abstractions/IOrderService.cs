using BookStore.Data.Models;
namespace BookStore.Logic.Abstractions;
public interface IOrderService
{
    Task<Order?> GetOrderByIdAsync(int id);
    Task<List<Order>> GetAllOrdersAsync();
    Task<List<Order>> GetOrdersByCustomerIdAsync(int customerId);
    Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status);
    Task CreateOrderAsync(Order order);
    Task UpdateOrderStatusAsync(int orderId, OrderStatus status);
    Task<decimal> CalculateOrderTotalAsync(int orderId);
    Task AddItemToOrderAsync(int orderId, int bookId, int quantity);
    Task RemoveItemFromOrderAsync(int orderId, int orderItemId);
    Task<List<OrderItem>> GetOrderItemsAsync(int orderId);
}
