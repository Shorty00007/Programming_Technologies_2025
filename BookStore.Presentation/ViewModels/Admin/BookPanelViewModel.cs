using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BookStore.Contracts;
using BookStore.Logic.Abstractions;
using BookStore.Presentation.Commands;
using System.Globalization;

namespace BookStore.Presentation.ViewModels.Admin;

public class BooksPanelViewModel : INotifyPropertyChanged
{
    private readonly IBookService _bookService;

    public ObservableCollection<BookDto> Books { get; } = new();

    private string _priceText = "";
    public string PriceText
    {
        get => _priceText;
        set
        {
            _priceText = value;
            OnPropertyChanged(nameof(PriceText));

            if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
            {
                Form.Price = parsed;
            }
        }
    }

    public BookDto? SelectedBook
    {
        get => _selectedBook;
        set
        {
            _selectedBook = value;
            OnPropertyChanged(nameof(SelectedBook));
            if (value != null)
            {
                PriceText = value.Price.ToString(CultureInfo.InvariantCulture);
                Form = new BookDto
                {
                    Id = value.Id,
                    Title = value.Title,
                    Author = value.Author,
                    ISBN = value.ISBN,
                    Price = value.Price,
                    Stock = value.Stock
                };
                PriceText = value.Price.ToString(CultureInfo.InvariantCulture);
                OnPropertyChanged(nameof(Form));
            }
        }
    }
    private BookDto? _selectedBook;

    public BookDto Form { get; set; } = new();

    public ICommand AddCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }

    public BooksPanelViewModel(IBookService bookService)
    {
        _bookService = bookService;

        AddCommand = new AsyncCommand(AddBookAsync);
        SaveCommand = new AsyncCommand(SaveBookAsync);
        DeleteCommand = new AsyncCommand(DeleteBookAsync);

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Books.Clear();
        var books = await _bookService.GetAllBooksAsync();
        foreach (var b in books)
            Books.Add(b);
    }

    private async Task AddBookAsync()
    {
        if (!ValidateForm()) return;

        try
        {
            var added = await _bookService.AddBookAsync(Form.Title, Form.Author, Form.ISBN, Form.Price, Form.Stock);
            Books.Add(added);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error adding book: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    private async Task SaveBookAsync()
    {
        if (SelectedBook == null || !ValidateForm()) return;

        try
        {
            var updated = await _bookService.EditBookAsync(
                SelectedBook.Id,
                Form.Title,
                Form.Author,
                Form.ISBN,
                Form.Price,
                Form.Stock);

            if (updated != null)
            {
                var index = Books.IndexOf(SelectedBook);
                Books[index] = updated;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving book: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task DeleteBookAsync()
    {
        if (SelectedBook == null) return;

        try
        {
            var removed = await _bookService.RemoveBookAsync(SelectedBook.Id);
            if (removed)
            {
                Books.Remove(SelectedBook);
                SelectedBook = null;
            }
            else
            {
                MessageBox.Show("Book could not be deleted.", "Delete Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("This book is associated with existing orders and cannot be deleted.\n\nDetails: " + ex.Message,
                            "Deletion Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(Form.Title) || Form.Title.Length > 100)
        {
            MessageBox.Show("Title is required and must be under 100 characters.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(Form.Author) || Form.Author.Length > 100)
        {
            MessageBox.Show("Author is required and must be under 100 characters.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(Form.ISBN) || Form.ISBN.Length < 10 || Form.ISBN.Length > 20 || !Form.ISBN.All(char.IsLetterOrDigit))
        {
            MessageBox.Show("ISBN must be 10–20 alphanumeric characters.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(PriceText) ||
    !decimal.TryParse(PriceText, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedPrice) ||
    parsedPrice < 0 ||
    PriceText.Contains(",") ||
    (parsedPrice * 100 % 1 != 0))
        {
            MessageBox.Show("Price must be a positive number with up to 2 decimal places (use '.' as decimal separator).",
                            "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }


        if (Form.Stock < 0)
        {
            MessageBox.Show("Stock cannot be negative.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
