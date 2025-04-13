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
    public async Task UpdateBookAsync(Book book)
    {
        var existingBook = await _context.Books
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.Id == book.Id);

        if (existingBook != null)
        {
            // Manual property updates
            existingBook.Title = book.Title;
            existingBook.Author = book.Author;
            existingBook.Pages = book.Pages;
            existingBook.Price = book.Price;

            // Handle categories if they're part of the update
            if (book.Categories != null)
            {
                // Clear existing categories and add new ones
                existingBook.Categories.Clear();
                foreach (var category in book.Categories)
                {
                    existingBook.Categories.Add(category);
                }
            }

            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Book>> GetTopSellingBooksAsync(int count)
    {
        // This implementation assumes OrderItems exist and are related to Books
        return await _context.Books
            .Include(b => b.Categories)
            .OrderByDescending(b => _context.OrderItems.Count(oi => oi.BookId == b.Id))
            .Take(count)
            .ToListAsync();
    }

    public async Task<decimal> GetBookPriceAsync(int id)
    {
        var book = await _context.Books.FindAsync(id);
        return book?.Price ?? 0;
    }

    public async Task<bool> IsBookInStockAsync(int id)
    {
        // Simple implementation that just checks if book exists
        var book = await _context.Books.FindAsync(id);
        return book != null;
    }
}
