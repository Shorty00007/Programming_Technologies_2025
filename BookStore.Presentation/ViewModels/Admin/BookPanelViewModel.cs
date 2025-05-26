using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BookStore.Contracts;
using BookStore.Logic.Abstractions;
using BookStore.Presentation.Commands;

namespace BookStore.Presentation.ViewModels.Admin;

public class BooksPanelViewModel : INotifyPropertyChanged
{
    private readonly IBookService _bookService;

    public ObservableCollection<BookDto> Books { get; } = new();
    public BookDto? SelectedBook
    {
        get => _selectedBook;
        set
        {
            _selectedBook = value;
            OnPropertyChanged(nameof(SelectedBook));
            if (value != null)
            {
                Form = new BookDto
                {
                    Id = value.Id,
                    Title = value.Title,
                    Author = value.Author,
                    ISBN = value.ISBN,
                    Price = value.Price,
                    Stock = value.Stock
                };
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
        var added = await _bookService.AddBookAsync(Form.Title, Form.Author, Form.ISBN, Form.Price, Form.Stock);
        Books.Add(added);
    }

    private async Task SaveBookAsync()
    {
        if (SelectedBook == null) return;
        var updated = await _bookService.EditBookAsync(SelectedBook.Id, Form.Title, Form.Author, Form.ISBN, Form.Price, Form.Stock);
        if (updated != null)
        {
            var index = Books.IndexOf(SelectedBook);
            Books[index] = updated;
        }
    }

    private async Task DeleteBookAsync()
    {
        if (SelectedBook == null) return;
        var removed = await _bookService.RemoveBookAsync(SelectedBook.Id);
        if (removed)
        {
            Books.Remove(SelectedBook);
            SelectedBook = null;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
