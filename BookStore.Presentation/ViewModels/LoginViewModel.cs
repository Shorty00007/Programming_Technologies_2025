using BookStore.Contracts;
using BookStore.Logic.Abstractions;
using System.Threading.Tasks;
using System.Windows.Input;
using BookStore.Presentation.Models;
using BookStore.Presentation.Commands;
using BookStore.Presentation.Helpers;
using BookStore.Presentation.Views;
using System.Text.RegularExpressions;
using System.Windows;

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

        var username = Form.Username;
        var password = Form.Password;

        if (string.IsNullOrWhiteSpace(username) || !Regex.IsMatch(username, @"^[a-zA-Z0-9]{1,16}$"))
        {
            MessageBox.Show("Username must be 1–16 characters long and contain only letters and numbers.",
                            "Invalid Username", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length > 16 || !Regex.IsMatch(password, @"^[\w\d\S]{1,16}$"))
        {
            MessageBox.Show("Password must be 1–16 characters long and can include letters, numbers, and special characters.",
                            "Invalid Password", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var user = _userService.Login(username, password);

            if (user != null)
            {
                _main.CurrentUser = user;

                if (user.Role == "Customer")
                    _main.LoadCustomerDashboard();
                else if (user.Role == "Admin")
                    _main.LoadAdminView();
            }
            else
            {
                MessageBox.Show("Login failed. Check your credentials.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Login error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

}
