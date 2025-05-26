using BookStore.Contracts;
using System;
using System.Collections.ObjectModel;

namespace BookStore.Presentation.ViewModels.Admin
{
    public class OrderDetailsViewModel
    {
        public string Username { get; }
        public int OrderId { get; }
        public DateTime OrderDate { get; }
        public decimal TotalAmount { get; }

        public ObservableCollection<OrderItemDto> Items { get; }

        public OrderDetailsViewModel(OrderDto order)
        {
            Username = order.Username;
            OrderId = order.Id;
            OrderDate = order.OrderDate;
            TotalAmount = order.TotalAmount;
            Items = new ObservableCollection<OrderItemDto>(order.Items);
        }
    }
}
