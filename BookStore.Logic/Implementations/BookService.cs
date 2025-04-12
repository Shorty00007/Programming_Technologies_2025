using BookStore.Data.Abstractions;
using BookStore.Data.Models;
using BookStore.Logic.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Logic.Implementations;

public class BookService : IBookService
{
    private readonly IBookStoreContext _context;

    public BookService(IBookStoreContext context)
    {
        _context = context;
    }

    public async Task AddBookAsync(Book book, List<int> categoryIds)
    {
        var categories = await _context.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync();

        book.Categories = categories;

        _context.Books.Add(book);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveBookAsync(int id)
    {
        var book = await _context.Books
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book != null)
        {
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Book?> GetBookByIdAsync(int id)
    {
        return await _context.Books
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<Book>> GetAllBooksAsync()
    {
        return await _context.Books
            .Include(b => b.Categories)
            .ToListAsync();
    }

    public async Task<List<Book>> GetBooksByCategoryIdAsync(int categoryId)
    {
        return await _context.Books
            .Include(b => b.Categories)
            .Where(b => b.Categories.Any(c => c.Id == categoryId))
            .ToListAsync();
    }

    public async Task<List<Book>> GetBooksByCategoryNameAsync(string categoryName)
    {
        return await _context.Books
            .Include(b => b.Categories)
            .Where(b => b.Categories.Any(c => c.Name == categoryName))
            .ToListAsync();
    }
}
