using BookStore.Data.Models;

namespace BookStore.Logic.Abstractions;

public interface IBookService
{
    Task AddBookAsync(Book book, List<int> categoryIds);
    Task RemoveBookAsync(int id);
    Task<Book?> GetBookByIdAsync(int id);
    Task<List<Book>> GetAllBooksAsync();
    Task<List<Book>> GetBooksByCategoryIdAsync(int categoryId);
    Task<List<Book>> GetBooksByCategoryNameAsync(string categoryName);
}
