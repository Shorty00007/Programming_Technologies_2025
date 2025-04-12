using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using BookStore.Data.Implementations;
using BookStore.Data.Models;

namespace BookStore.Tests;

[TestClass]
public class DataLayerTests
{
    private BookStoreContext _context = null!;
    private string _connectionString = null!;
    private string _dbName = null!;

    [TestInitialize]
    public void Init()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        _connectionString = config.GetConnectionString("DefaultConnection")!;
        _dbName = new SqlConnectionStringBuilder(_connectionString).InitialCatalog!;

        var options = new DbContextOptionsBuilder<BookStoreContext>()
            .UseSqlServer(_connectionString)
            .Options;

        _context = new BookStoreContext(options);
        _context.Database.EnsureCreated();
    }

    [TestCleanup]
    public void Cleanup()
    {
        var masterConnection = _connectionString.Replace(_dbName, "master");

        using var conn = new SqlConnection(masterConnection);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = $@"
            IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{_dbName}')
            BEGIN
                ALTER DATABASE [{_dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{_dbName}];
            END";

        try
        {
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cleanup failed: {ex.Message}");
        }
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanAddCategory()
    {
        var category = new Category { Name = "Programming" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var result = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Programming");
        Assert.IsNotNull(result);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanAddBook()
    {
        var book = new Book
        {
            Title = "C# in Depth",
            Author = "Jon Skeet",
            Pages = 900,
            Price = 49.99M
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        var result = await _context.Books.FirstOrDefaultAsync(b => b.Title == "C# in Depth");
        Assert.IsNotNull(result);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanAssignCategoriesToBook()
    {
        var cat1 = new Category { Name = "Tech" };
        var cat2 = new Category { Name = "Backend" };
        _context.Categories.AddRange(cat1, cat2);
        await _context.SaveChangesAsync();

        var book = new Book
        {
            Title = "Clean Architecture",
            Author = "Robert C. Martin",
            Pages = 400,
            Price = 39.99M,
            Categories = new List<Category> { cat1, cat2 }
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        var result = await _context.Books
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.Title == "Clean Architecture");

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result!.Categories.Count);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanGetAllBooks()
    {
        var book1 = new Book { Title = "Book A", Author = "Author A", Pages = 100, Price = 10 };
        var book2 = new Book { Title = "Book B", Author = "Author B", Pages = 200, Price = 20 };
        _context.Books.AddRange(book1, book2);
        await _context.SaveChangesAsync();

        var books = await _context.Books.ToListAsync();
        Assert.AreEqual(2, books.Count);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanGetAllCategories()
    {
        var cat1 = new Category { Name = "A" };
        var cat2 = new Category { Name = "B" };
        _context.Categories.AddRange(cat1, cat2);
        await _context.SaveChangesAsync();

        var categories = await _context.Categories.ToListAsync();
        Assert.AreEqual(2, categories.Count);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanQueryBooksByCategory()
    {
        var category = new Category { Name = "QueryCat" };
        var book = new Book
        {
            Title = "QueryBook",
            Author = "Query Author",
            Pages = 123,
            Price = 19.99M,
            Categories = new List<Category> { category }
        };

        _context.Categories.Add(category);
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        var books = await _context.Books
            .Include(b => b.Categories)
            .Where(b => b.Categories.Any(c => c.Name == "QueryCat"))
            .ToListAsync();

        Assert.AreEqual(1, books.Count);
        Assert.AreEqual("QueryBook", books[0].Title);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanUpdateBookTitle()
    {
        var book = new Book { Title = "Old Title", Author = "Author", Pages = 100, Price = 9.99M };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        book.Title = "New Title";
        await _context.SaveChangesAsync();

        var updated = await _context.Books.FirstAsync(b => b.Id == book.Id);
        Assert.AreEqual("New Title", updated.Title);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanUpdateCategoryName()
    {
        var category = new Category { Name = "OldCat" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        category.Name = "NewCat";
        await _context.SaveChangesAsync();

        var updated = await _context.Categories.FirstAsync(c => c.Id == category.Id);
        Assert.AreEqual("NewCat", updated.Name);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanRemoveBook()
    {
        var book = new Book { Title = "ToRemove", Author = "Author", Pages = 111, Price = 11.11M };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        var exists = await _context.Books.AnyAsync(b => b.Id == book.Id);
        Assert.IsFalse(exists);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanRemoveCategory()
    {
        var category = new Category { Name = "ToRemoveCat" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        var exists = await _context.Categories.AnyAsync(c => c.Id == category.Id);
        Assert.IsFalse(exists);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanAddBookWithNoCategories()
    {
        var book = new Book
        {
            Title = "NoCatBook",
            Author = "Author",
            Pages = 100,
            Price = 9.99M
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        var result = await _context.Books.FirstOrDefaultAsync(b => b.Title == "NoCatBook");
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result!.Categories.Count);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanAddCategoryWithNoBooks()
    {
        var category = new Category { Name = "EmptyCat" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var result = await _context.Categories.Include(c => c.Books).FirstOrDefaultAsync(c => c.Name == "EmptyCat");
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result!.Books.Count);
    }
}
