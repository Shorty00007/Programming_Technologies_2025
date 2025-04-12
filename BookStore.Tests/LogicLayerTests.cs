using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using BookStore.Data.Implementations;
using BookStore.Data.Models;
using BookStore.Logic.Abstractions;
using BookStore.Logic.Implementations;
using Microsoft.Data.SqlClient;

namespace BookStore.Tests;

[TestClass]
public class LogicLayerTests
{
    private BookStoreContext _context = null!;
    private IBookService _bookService = null!;
    private ICategoryService _categoryService = null!;
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
        _bookService = new BookService(_context);
        _categoryService = new CategoryService(_context);
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
    public async Task CanAddAndFetchBookWithCategories()
    {
        var cat1 = new Category { Name = "Architecture" };
        var cat2 = new Category { Name = "Design" };

        await _categoryService.AddCategoryAsync(cat1);
        await _categoryService.AddCategoryAsync(cat2);

        var categories = await _categoryService.GetAllCategoriesAsync();
        var categoryIds = categories.Select(c => c.Id).ToList();

        var book = new Book
        {
            Title = "Clean Architecture",
            Author = "Robert C. Martin",
            Pages = 400,
            Price = 39.99M
        };

        await _bookService.AddBookAsync(book, categoryIds);

        var allBooks = await _bookService.GetAllBooksAsync();
        var fetched = allBooks.FirstOrDefault(b => b.Title == "Clean Architecture");

        Assert.IsNotNull(fetched);
        Assert.AreEqual(2, fetched!.Categories.Count);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanGetBookById()
    {
        var book = new Book
        {
            Title = "DDD",
            Author = "Eric Evans",
            Pages = 500,
            Price = 59.99M
        };

        await _bookService.AddBookAsync(book, new List<int>());

        var allBooks = await _bookService.GetAllBooksAsync();
        var bookId = allBooks.First().Id;

        var fetched = await _bookService.GetBookByIdAsync(bookId);
        Assert.IsNotNull(fetched);
        Assert.AreEqual("DDD", fetched!.Title);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanRemoveBook()
    {
        var book = new Book { Title = "Temp Book", Author = "Anon", Pages = 123, Price = 9.99M };
        await _bookService.AddBookAsync(book, new List<int>());

        var allBooks = await _bookService.GetAllBooksAsync();
        var id = allBooks.First().Id;

        await _bookService.RemoveBookAsync(id);

        var after = await _bookService.GetAllBooksAsync();
        Assert.AreEqual(0, after.Count);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanAddAndRemoveCategory()
    {
        var category = new Category { Name = "Disposable" };
        await _categoryService.AddCategoryAsync(category);

        var all = await _categoryService.GetAllCategoriesAsync();
        Assert.AreEqual(1, all.Count);

        await _categoryService.RemoveCategoryAsync(all[0].Id);

        var after = await _categoryService.GetAllCategoriesAsync();
        Assert.AreEqual(0, after.Count);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanGetBooksByCategoryId()
    {
        var cat = new Category { Name = "QueryCat" };
        await _categoryService.AddCategoryAsync(cat);
        var catId = (await _categoryService.GetAllCategoriesAsync()).First().Id;

        var book = new Book { Title = "Targeted", Author = "Test", Pages = 123, Price = 10.00M };
        await _bookService.AddBookAsync(book, new List<int> { catId });

        var results = await _bookService.GetBooksByCategoryIdAsync(catId);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("Targeted", results[0].Title);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanGetBooksByCategoryName()
    {
        var cat = new Category { Name = "ByNameCat" };
        await _categoryService.AddCategoryAsync(cat);
        var catId = (await _categoryService.GetAllCategoriesAsync()).First().Id;

        var book = new Book { Title = "CatNamedBook", Author = "Someone", Pages = 222, Price = 20 };
        await _bookService.AddBookAsync(book, new List<int> { catId });

        var results = await _bookService.GetBooksByCategoryNameAsync("ByNameCat");
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("CatNamedBook", results[0].Title);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanRemoveCategoryAssignedToBook_AndUnlinkIt()
    {
        var category = new Category { Name = "Temporary Category" };
        await _categoryService.AddCategoryAsync(category);

        var catId = (await _categoryService.GetAllCategoriesAsync()).First().Id;

        var book = new Book
        {
            Title = "Linked Book",
            Author = "Test Author",
            Pages = 123,
            Price = 19.99M
        };

        await _bookService.AddBookAsync(book, new List<int> { catId });

        var linkedBook = (await _bookService.GetAllBooksAsync()).First();
        Assert.AreEqual(1, linkedBook.Categories.Count);

        await _categoryService.RemoveCategoryAsync(catId);

        var categories = await _categoryService.GetAllCategoriesAsync();
        Assert.IsFalse(categories.Any(c => c.Id == catId));

        var remainingBook = (await _bookService.GetAllBooksAsync()).First();
        Assert.AreEqual("Linked Book", remainingBook.Title);
        Assert.AreEqual(0, remainingBook.Categories.Count);
    }

}
