using BookStore.Data.Models;
namespace BookStore.Logic.Abstractions;
public interface ICategoryService
{
    Task<List<Category>> GetAllCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task AddCategoryAsync(Category category);
    Task RemoveCategoryAsync(int id);
    Task UpdateCategoryAsync(Category category);
    Task<List<Category>> GetCategoriesByBookIdAsync(int bookId);
    Task<int> GetCategoryBooksCountAsync(int categoryId);
}
