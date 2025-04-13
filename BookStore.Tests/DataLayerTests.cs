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
    [TestMethod]
    [DoNotParallelize]
    public async Task CanCreateAndRetrieveCustomer()
    {
        // Arrange
        var customer = new Customer
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "555-123-4567",
            RegistrationDate = DateTime.Now
        };

        // Act
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedCustomer = await _context.Customers.FindAsync(customer.Id);
        Assert.IsNotNull(retrievedCustomer);
        Assert.AreEqual("John Doe", retrievedCustomer!.Name);
        Assert.AreEqual("john.doe@example.com", retrievedCustomer.Email);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanCreateOrderWithItems()
    {
        // Arrange
        var customer = new Customer
        {
            Name = "Jane Smith",
            Email = "jane.smith@example.com",
            PhoneNumber = "555-987-6543",
            RegistrationDate = DateTime.Now
        };

        var book1 = new Book { Title = "Test Book 1", Author = "Author X", Pages = 300, Price = 29.99m };
        var book2 = new Book { Title = "Test Book 2", Author = "Author Y", Pages = 250, Price = 19.99m };

        _context.Customers.Add(customer);
        _context.Books.AddRange(book1, book2);
        await _context.SaveChangesAsync();

        var order = new Order
        {
            CustomerId = customer.Id,
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            TotalAmount = book1.Price + book2.Price
        };

        var orderItem1 = new OrderItem
        {
            BookId = book1.Id,
            Quantity = 1,
            UnitPrice = book1.Price
        };

        var orderItem2 = new OrderItem
        {
            BookId = book2.Id,
            Quantity = 1,
            UnitPrice = book2.Price
        };

        order.OrderItems = new List<OrderItem> { orderItem1, orderItem2 };

        // Act
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedOrder = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.CustomerId == customer.Id);

        Assert.IsNotNull(retrievedOrder);
        Assert.AreEqual(OrderStatus.Pending, retrievedOrder!.Status);
        Assert.AreEqual(2, retrievedOrder.OrderItems.Count);
        Assert.AreEqual(49.98m, retrievedOrder.TotalAmount);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanRetrieveOrdersForCustomer()
    {
        // Arrange
        var customer = new Customer
        {
            Name = "Robert Johnson",
            Email = "robert.johnson@example.com",
            PhoneNumber = "555-456-7890",
            RegistrationDate = DateTime.Now
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var book = new Book { Title = "SQL Mastery", Author = "Database Pro", Pages = 450, Price = 49.99m };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        var order1 = new Order
        {
            CustomerId = customer.Id,
            OrderDate = DateTime.Now.AddDays(-5),
            Status = OrderStatus.Delivered,
            TotalAmount = book.Price
        };

        var order2 = new Order
        {
            CustomerId = customer.Id,
            OrderDate = DateTime.Now.AddDays(-2),
            Status = OrderStatus.Shipped,
            TotalAmount = book.Price * 2
        };

        var orderItem1 = new OrderItem
        {
            Order = order1,
            BookId = book.Id,
            Quantity = 1,
            UnitPrice = book.Price
        };

        var orderItem2 = new OrderItem
        {
            Order = order2,
            BookId = book.Id,
            Quantity = 2,
            UnitPrice = book.Price
        };

        order1.OrderItems = new List<OrderItem> { orderItem1 };
        order2.OrderItems = new List<OrderItem> { orderItem2 };

        _context.Orders.AddRange(order1, order2);
        await _context.SaveChangesAsync();

        // Act
        var customerWithOrders = await _context.Customers
            .Include(c => c.Orders)
            .ThenInclude(o => o.OrderItems)
            .FirstOrDefaultAsync(c => c.Id == customer.Id);

        // Assert
        Assert.IsNotNull(customerWithOrders);
        Assert.AreEqual(2, customerWithOrders!.Orders.Count);
        Assert.AreEqual(OrderStatus.Delivered, customerWithOrders.Orders.First().Status);
        Assert.AreEqual(OrderStatus.Shipped, customerWithOrders.Orders.Skip(1).First().Status);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanUpdateOrderStatus()
    {
        // Arrange
        var customer = new Customer
        {
            Name = "Mary Williams",
            Email = "mary.williams@example.com",
            PhoneNumber = "555-222-3333",
            RegistrationDate = DateTime.Now
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var order = new Order
        {
            CustomerId = customer.Id,
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            TotalAmount = 59.99m
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var retrievedOrder = await _context.Orders.FindAsync(order.Id);
        retrievedOrder!.Status = OrderStatus.Shipped;
        await _context.SaveChangesAsync();

        // Assert
        var updatedOrder = await _context.Orders.FindAsync(order.Id);
        Assert.IsNotNull(updatedOrder);
        Assert.AreEqual(OrderStatus.Shipped, updatedOrder!.Status);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanCalculateOrderTotalFromItems()
    {
        // Arrange
        var customer = new Customer
        {
            Name = "Test Customer",
            Email = "test@example.com",
            PhoneNumber = "555-123-4567",
            RegistrationDate = DateTime.Now
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var book1 = new Book { Title = "Book 1", Author = "Author 1", Pages = 100, Price = 10.99m };
        var book2 = new Book { Title = "Book 2", Author = "Author 2", Pages = 200, Price = 15.99m };
        var book3 = new Book { Title = "Book 3", Author = "Author 3", Pages = 300, Price = 20.99m };
        _context.Books.AddRange(book1, book2, book3);
        await _context.SaveChangesAsync();

        // Create order first
        var order = new Order
        {
            CustomerId = customer.Id,
            Customer = customer,
            OrderDate = DateTime.Now,
            Status = OrderStatus.Processing,
            TotalAmount = 0 // Will update this after adding items
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Now add order items with proper references
        var orderItems = new List<OrderItem>
    {
        new OrderItem {
            OrderId = order.Id,
            BookId = book1.Id,
            Quantity = 2,
            UnitPrice = book1.Price
        },
        new OrderItem {
            OrderId = order.Id,
            BookId = book2.Id,
            Quantity = 1,
            UnitPrice = book2.Price
        },
        new OrderItem {
            OrderId = order.Id,
            BookId = book3.Id,
            Quantity = 3,
            UnitPrice = book3.Price
        }
    };

        _context.OrderItems.AddRange(orderItems);
        await _context.SaveChangesAsync();

        // Expected total: (10.99 * 2) + (15.99 * 1) + (20.99 * 3) = 21.98 + 15.99 + 62.97 = 100.94
        decimal expectedTotal = (book1.Price * 2) + (book2.Price * 1) + (book3.Price * 3);

        // Update order total
        order.TotalAmount = expectedTotal;
        await _context.SaveChangesAsync();

        // Assert
        var retrievedOrder = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        Assert.IsNotNull(retrievedOrder);
        Assert.AreEqual(6, retrievedOrder!.OrderItems.Sum(i => i.Quantity));
        Assert.AreEqual(expectedTotal, retrievedOrder.TotalAmount);
        Assert.AreEqual(100.94m, retrievedOrder.TotalAmount);
    }

}
