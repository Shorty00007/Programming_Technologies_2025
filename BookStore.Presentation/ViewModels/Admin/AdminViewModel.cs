using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using BookStore.Presentation.Commands;
using BookStore.Logic.Abstractions;
using BookStore.Presentation.ViewModels.Admin;
using BookStore.Presentation.Views.Admin;

namespace BookStore.Presentation.ViewModels.Admin;

public class AdminViewModel : INotifyPropertyChanged
{
    private readonly IBookService _bookService;
    private readonly IOrderService _orderService;
    private readonly IProcessStateService _processStateService;
    private readonly IEventLogService _eventLogService;

    public ICommand ShowBooksCommand { get; }
    public ICommand ShowOrdersCommand { get; }
    public ICommand ShowProcessStateCommand { get; }
    public ICommand ShowLogsCommand { get; }

    public AdminViewModel(
        IBookService bookService,
        IOrderService orderService,
        IProcessStateService processStateService,
        IEventLogService eventLogService)
    {
        _bookService = bookService;
        _orderService = orderService;
        _processStateService = processStateService;
        _eventLogService = eventLogService;

        ShowBooksCommand = new RelayCommand(LoadBooksPanel);
        ShowOrdersCommand = new RelayCommand(LoadOrdersPanel);
        ShowProcessStateCommand = new RelayCommand(LoadProcessPanel);
        ShowLogsCommand = new RelayCommand(LoadLogsPanel);

        LoadBooksPanel(); // Default view
    }

    private UserControl? _currentPanel;
    public UserControl? CurrentPanel
    {
        get => _currentPanel;
        set
        {
            _currentPanel = value;
            OnPropertyChanged(nameof(CurrentPanel));
        }
    }

    private void LoadBooksPanel()
    {
        CurrentPanel = new BooksPanel
        {
            DataContext = new BooksPanelViewModel(_bookService)
        };
    }

    private void LoadOrdersPanel()
    {
        CurrentPanel = new OrdersPanel
        {
            DataContext = new OrdersPanelViewModel(_orderService)
        };
    }

    private void LoadProcessPanel()
    {
        CurrentPanel = new ProcessStatePanel
        {
            DataContext = new ProcessStateViewModel(_processStateService)
        };
    }

    private void LoadLogsPanel()
    {
        CurrentPanel = new LogsPanel
        {
            DataContext = new LogsViewModel(_eventLogService)
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
