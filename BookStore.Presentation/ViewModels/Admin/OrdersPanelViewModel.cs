using BookStore.Contracts;
using BookStore.Logic.Abstractions;
using BookStore.Presentation.Commands;
using BookStore.Presentation.ViewModels.Admin;
using BookStore.Presentation.Views.Admin;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace BookStore.Presentation.ViewModels.Admin;
public class OrdersPanelViewModel : INotifyPropertyChanged
{
    private readonly IOrderService _orderService;

    public ObservableCollection<OrderDto> Orders { get; } = new();

    public OrderDto? SelectedOrder
    {
        get => _selectedOrder;
        set
        {
            _selectedOrder = value;
            OnPropertyChanged(nameof(SelectedOrder));
        }
    }
    private OrderDto? _selectedOrder;

    public ICommand ViewDetailsCommand { get; }

    public OrdersPanelViewModel(IOrderService orderService)
    {
        _orderService = orderService;

        ViewDetailsCommand = new AsyncCommand(ViewDetailsAsync);
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Orders.Clear();
        var list = await _orderService.GetAllOrdersAsync();
        foreach (var order in list)
            Orders.Add(order);
    }

    private async Task ViewDetailsAsync()
    {
        if (SelectedOrder == null) return;

        var details = await _orderService.GetOrderDetailsAsync(SelectedOrder.Id);
        if (details != null)
        {
            var dialog = new OrderDetailsView
            {
                DataContext = new OrderDetailsViewModel(details)
            };
            dialog.ShowDialog(); // ✅ opens from ViewModel (still UI logic, but controlled)
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
