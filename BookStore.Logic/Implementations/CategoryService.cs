using BookStore.Data.Abstractions;
using BookStore.Data.Models;
using BookStore.Logic.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Logic.Implementations;

public class CategoryService : ICategoryService
{
    private readonly IBookStoreContext _context;

    public CategoryService(IBookStoreContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _context.Categories
            .Include(c => c.Books)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories
            .Include(c => c.Books)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddCategoryAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveCategoryAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Books)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category != null)
        {
            // Remove the links between books and this category
            foreach (var book in category.Books.ToList())
            {
                book.Categories.Remove(category);
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
    public async Task UpdateCategoryAsync(Category category)
    {
        var existingCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == category.Id);

        if (existingCategory != null)
        {
            existingCategory.Name = category.Name;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Category>> GetCategoriesByBookIdAsync(int bookId)
    {
        return await _context.Categories
            .Where(c => c.Books.Any(b => b.Id == bookId))
            .ToListAsync();
    }

    public async Task<int> GetCategoryBooksCountAsync(int categoryId)
    {
        var category = await _context.Categories
            .Include(c => c.Books)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        return category?.Books.Count ?? 0;
    }
}
