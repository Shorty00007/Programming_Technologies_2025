using BookStore.Contracts;
using BookStore.Data.Models;
using BookStore.Logic.Abstractions;
using System.Collections.Concurrent;

namespace BookStore.LogicTests.helper;
public class MockBookService : IBookService
{
    private readonly List<BookDto> _books = new();
    private readonly List<EventLog> _eventLogs = new();
    private readonly List<ProcessState> _processStates = new();
    private int _idCounter = 1;

    public Task<BookDto> AddBookAsync(string title, string author, string isbn, decimal price, int stock)
    {
        var book = new BookDto
        {
            Id = _idCounter++,
            Title = title,
            Author = author,
            ISBN = isbn,
            Price = price,
            Stock = stock
        };

        _books.Add(book);
        LogEvent("Book Added", $"Added book '{title}' (ISBN: {isbn})");
        UpdateProcessState();

        return Task.FromResult(book);
    }

    public Task<bool> RemoveBookAsync(int id)
    {
        var book = _books.FirstOrDefault(b => b.Id == id);
        if (book == null) return Task.FromResult(false);

        _books.Remove(book);
        LogEvent("Book Removed", $"Removed book '{book.Title}' (ISBN: {book.ISBN})");
        UpdateProcessState();

        return Task.FromResult(true);
    }

    public Task<BookDto?> EditBookAsync(int id, string title, string author, string isbn, decimal price, int stock)
    {
        var book = _books.FirstOrDefault(b => b.Id == id);
        if (book == null) return Task.FromResult<BookDto?>(null);

        book.Title = title;
        book.Author = author;
        book.ISBN = isbn;
        book.Price = price;
        book.Stock = stock;

        LogEvent("Book Edited", $"Edited book '{title}' (ISBN: {isbn})");
        UpdateProcessState();

        return Task.FromResult<BookDto?>(book);
    }

    public Task<IEnumerable<BookDto>> GetAllBooksAsync()
    {
        return Task.FromResult<IEnumerable<BookDto>>(_books);
    }

    public Task<IEnumerable<BookDto>> GetBooksByAuthorAsync(string author)
    {
        var result = _books.Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult<IEnumerable<BookDto>>(result);
    }

    public Task<BookDto?> GetBookByIsbnAsync(string isbn)
    {
        var book = _books.FirstOrDefault(b => b.ISBN == isbn);
        return Task.FromResult<BookDto?>(book);
    }

    private void LogEvent(string type, string desc, int? userId = null)
    {
        _eventLogs.Add(new EventLog
        {
            EventType = type,
            Description = desc,
            UserId = userId
        });
    }

    public Task UpdateProcessStateAsync()
    {
        UpdateProcessState();
        return Task.CompletedTask;
    }

    private void UpdateProcessState()
    {
        var totalStock = _books.Sum(b => b.Stock);
        var totalOrders = 0; // use mocked data if available
        var totalRevenue = 0m; // use mocked data if available

        _processStates.Add(new ProcessState
        {
            TotalBooksInStock = totalStock,
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue
        });
    }
}
