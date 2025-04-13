using BookStore.Data.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BookStore.Logic.Abstractions;

public interface IBookService
{
    Task AddBookAsync(Book book, List<int> categoryIds);
    Task RemoveBookAsync(int id);
    Task<Book?> GetBookByIdAsync(int id);
    Task<List<Book>> GetAllBooksAsync();
    Task<List<Book>> GetBooksByCategoryIdAsync(int categoryId);
    Task<List<Book>> GetBooksByCategoryNameAsync(string categoryName);
    Task UpdateBookAsync(Book book);
    Task<List<Book>> GetTopSellingBooksAsync(int count);
    Task<decimal> GetBookPriceAsync(int id);
    Task<bool> IsBookInStockAsync(int id);
}
