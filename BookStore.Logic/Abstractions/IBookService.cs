using BookStore.Contracts;
using BookStore.Data.Models;

public interface IBookService
{
    Task<BookDto> AddBookAsync(string title, string author, string isbn, decimal price, int stock);
    Task<bool> RemoveBookAsync(int id);
    Task<BookDto?> EditBookAsync(int id, string title, string author, string isbn, decimal price, int stock);
    Task<IEnumerable<BookDto>> GetAllBooksAsync();
    Task<IEnumerable<BookDto>> GetBooksByAuthorAsync(string author);
    Task<BookDto?> GetBookByIsbnAsync(string isbn);
    Task UpdateProcessStateAsync();
}