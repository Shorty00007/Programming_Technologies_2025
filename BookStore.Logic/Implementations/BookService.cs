using BookStore.Contracts;
using BookStore.Data.Abstractions;
using BookStore.Data.Models;
using BookStore.Logic.Abstractions;
using Microsoft.EntityFrameworkCore;

public class BookService : IBookService
{
    private readonly IBookStoreContext _context;

    public BookService(IBookStoreContext context)
    {
        _context = context;
    }

    public async Task<BookDto> AddBookAsync(string title, string author, string isbn, decimal price, int stock)
    {
        var book = new Book
        {
            Title = title,
            Author = author,
            ISBN = isbn,
            Price = price,
            Stock = stock
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        await LogEventAsync("Book Added", $"Added book '{title}' (ISBN: {isbn})");
        await UpdateProcessStateAsync();

        return ToDto(book);
    }

    public async Task<bool> RemoveBookAsync(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null) return false;

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        await LogEventAsync("Book Removed", $"Removed book '{book.Title}' (ISBN: {book.ISBN})");
        await UpdateProcessStateAsync();

        return true;
    }

    public async Task<BookDto?> EditBookAsync(int id, string title, string author, string isbn, decimal price, int stock)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null) return null;

        book.Title = title;
        book.Author = author;
        book.ISBN = isbn;
        book.Price = price;
        book.Stock = stock;

        await _context.SaveChangesAsync();

        await LogEventAsync("Book Edited", $"Edited book '{title}' (ISBN: {isbn})");
        await UpdateProcessStateAsync();

        return ToDto(book);
    }

    public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
    {
        return await _context.Books
            .Select(b => ToDto(b))
            .ToListAsync();
    }

    public async Task<IEnumerable<BookDto>> GetBooksByAuthorAsync(string author)
    {
        return await _context.Books
            .Where(b => b.Author.ToLower().Contains(author.ToLower()))
            .Select(b => ToDto(b))
            .ToListAsync();
    }

    public async Task<BookDto?> GetBookByIsbnAsync(string isbn)
    {
        var book = await _context.Books.FirstOrDefaultAsync(b => b.ISBN == isbn);
        return book == null ? null : ToDto(book);
    }

    private static BookDto ToDto(Book book) => new BookDto
    {
        Id = book.Id,
        Title = book.Title,
        Author = book.Author,
        ISBN = book.ISBN,
        Price = book.Price,
        Stock = book.Stock
    };

    private async Task LogEventAsync(string eventType, string description, int? userId = null)
    {
        var log = new EventLog
        {
            EventType = eventType,
            Description = description,
            UserId = userId
        };

        _context.EventLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateProcessStateAsync()
    {
        var totalBooksInStock = await _context.Books.SumAsync(b => b.Stock);
        var totalOrders = await _context.Orders.CountAsync();
        var totalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount);

        var snapshot = new ProcessState
        {
            TotalBooksInStock = totalBooksInStock,
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue
        };

        _context.ProcessStates.Add(snapshot);
        await _context.SaveChangesAsync();
    }
}
