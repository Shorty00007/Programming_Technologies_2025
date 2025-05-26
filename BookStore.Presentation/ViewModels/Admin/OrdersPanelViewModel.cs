using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using BookStore.Contracts;
using BookStore.Logic.Abstractions;

namespace BookStore.Presentation.ViewModels.Admin;

public class OrdersPanelViewModel : INotifyPropertyChanged
{
    private readonly IOrderService _orderService;

    public ObservableCollection<OrderDto> Orders { get; } = new();

    public OrdersPanelViewModel(IOrderService orderService)
    {
        _orderService = orderService;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Orders.Clear();
        var list = await _orderService.GetAllOrdersAsync();
        foreach (var order in list)
            Orders.Add(order);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
