using BookStore.Contracts;
using BookStore.Logic.Abstractions;
using System.Threading.Tasks;
using System.Windows.Input;
using BookStore.Presentation.Models;
using BookStore.Presentation.Commands;
using BookStore.Presentation.Helpers;
using BookStore.Presentation.Views;

namespace BookStore.Presentation.ViewModels;

public class LoginViewModel
{
    private readonly IUserService _userService;
    private readonly MainWindowViewModel _main;

    public LoginFormModel Form { get; set; } = new();
    public ICommand LoginCommand { get; }
    public ICommand GoToRegisterCommand { get; }

    public LoginViewModel(IUserService userService, MainWindowViewModel main)
    {
        _userService = userService;
        _main = main;

        LoginCommand = new AsyncCommand(LoginAsync);
        GoToRegisterCommand = new RelayCommand(() => _main.LoadRegisterView());
    }

    private async Task LoginAsync()
    {
        await Task.Yield();

        var user = _userService.Login(Form.Username, Form.Password);

        if (user != null)
        {
            _main.CurrentUser = user;

            if (user?.Role == "Customer")
                _main.LoadCustomerDashboard();
            else if (user?.Role == "Admin")
                _main.LoadAdminView();

        }
    }
}
