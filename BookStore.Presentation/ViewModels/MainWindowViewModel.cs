using BookStore.Logic.Abstractions;
using System.ComponentModel;
using System.Windows.Controls;
using BookStore.Presentation.Views;
using BookStore.Presentation.ViewModels;
using BookStore.Contracts;
using BookStore.Presentation.ViewModels.Admin;
using BookStore.Presentation.Views.Admin;

namespace BookStore.Presentation.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public UserDto? CurrentUser { get; set; }

    private readonly IUserService _userService;
    private readonly IBookService _bookService;
    private readonly IOrderService _orderService;
    private readonly IEventLogService _eventLogService;
    private readonly IProcessStateService _processStateService;


    private UserControl? _currentView;
    public UserControl? CurrentView
    {
        get => _currentView;
        set
        {
            _currentView = value;
            OnPropertyChanged(nameof(CurrentView));
        }
    }

    public MainWindowViewModel(
    IUserService userService,
    IBookService bookService,
    IOrderService orderService,
    IEventLogService eventLogService,
    IProcessStateService processStateService)
    {
        _userService = userService;
        _bookService = bookService;
        _orderService = orderService;
        _eventLogService = eventLogService;
        _processStateService = processStateService;

        LoadLoginView();
    }

    public void LoadLoginView()
    {
        var view = new LoginView();
        view.DataContext = new LoginViewModel(_userService, this);
        CurrentView = view;
    }


    public void LoadRegisterView()
    {
        var view = new RegisterView();
        view.DataContext = new RegisterViewModel(_userService, this);
        CurrentView = view;
    }

    public void LoadCustomerDashboard()
    {
        var view = new CustomerDashboardView();
        view.DataContext = new CustomerDashboardViewModel(_bookService, _orderService, CurrentUser!);
        CurrentView = view;
    }

    public void LoadAdminView()
    {
        var view = new AdminView();
        view.DataContext = new AdminViewModel(
            _bookService,
            _orderService,
            _processStateService,
            _eventLogService);
        CurrentView = view;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
