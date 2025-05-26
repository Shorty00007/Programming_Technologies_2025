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
public class RegisterViewModel
{
    private readonly IUserService _userService;
    private readonly MainWindowViewModel _main;

    public RegisterFormModel Form { get; set; } = new();
    public ICommand RegisterCommand { get; }
    public ICommand GoToLoginCommand { get; }


    public RegisterViewModel(IUserService userService, MainWindowViewModel main)
    {
        _userService = userService;
        _main = main;

        RegisterCommand = new AsyncCommand(RegisterAsync);
        GoToLoginCommand = new RelayCommand(() => _main.LoadLoginView());
    }


    private async Task RegisterAsync()
    {
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
            var user = await _userService.Register(username, password);
            if (user != null)
                _main.LoadLoginView();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Registration failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
