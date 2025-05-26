using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookStore.Contracts;

namespace BookStore.Logic.Abstractions
{
    public interface IOrderService
    {
        Task<OrderDto> PlaceOrderAsync(int userId, List<(int bookId, int quantity)> items);
        Task<IEnumerable<OrderDto>> GetOrdersForUserAsync(int userId);
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto?> GetOrderDetailsAsync(int orderId);
    }

}
