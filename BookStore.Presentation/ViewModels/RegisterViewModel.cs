using BookStore.Contracts;
using BookStore.Logic.Abstractions;
using System.Threading.Tasks;
using System.Windows.Input;
using BookStore.Presentation.Models;
using BookStore.Presentation.Commands;
using BookStore.Presentation.Helpers;
using BookStore.Presentation.Views;

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
        System.Diagnostics.Debug.WriteLine($"Password entered: '{Form.Password}'");

        var user = await _userService.Register(Form.Username, Form.Password);
        if (user != null)
            _main.LoadLoginView();
    }
}
