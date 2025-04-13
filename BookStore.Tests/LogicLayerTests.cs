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
    private ICustomerService _customerService = null!;
    private IOrderService _orderService = null!;
    private IOrderItemService _orderItemService = null!;
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
        _customerService = new CustomerService(_context);
        _orderService = new OrderService(_context);
        _orderItemService = new OrderItemService(_context);
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
    [TestMethod]
    [DoNotParallelize]
    public async Task CanUpdateBook()
    {
        var book = new Book
        {
            Title = "Original Title",
            Author = "Original Author",
            Pages = 100,
            Price = 19.99M
        };
        await _bookService.AddBookAsync(book, new List<int>());

        book.Title = "Updated Title";
        book.Author = "Updated Author";
        book.Pages = 200;
        book.Price = 29.99M;
        await _bookService.UpdateBookAsync(book);

        var updatedBook = await _bookService.GetBookByIdAsync(book.Id);
        Assert.IsNotNull(updatedBook);
        Assert.AreEqual("Updated Title", updatedBook!.Title);
        Assert.AreEqual("Updated Author", updatedBook.Author);
        Assert.AreEqual(200, updatedBook.Pages);
        Assert.AreEqual(29.99M, updatedBook.Price);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanGetBookPrice()
    {
        var book = new Book
        {
            Title = "Price Test Book",
            Author = "Test Author",
            Pages = 200,
            Price = 24.99M
        };
        await _bookService.AddBookAsync(book, new List<int>());

        var price = await _bookService.GetBookPriceAsync(book.Id);

        Assert.AreEqual(24.99M, price);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanCheckBookInStock()
    {
        var book = new Book
        {
            Title = "In Stock Book",
            Author = "Stock Author",
            Pages = 150,
            Price = 15.99M
        };
        await _bookService.AddBookAsync(book, new List<int>());

        var inStock = await _bookService.IsBookInStockAsync(book.Id);
        var notInStock = await _bookService.IsBookInStockAsync(9999);

        Assert.IsTrue(inStock);
        Assert.IsFalse(notInStock);
    }
    [TestMethod]
    [DoNotParallelize]
    public async Task CanUpdateCategory()
    {
        var category = new Category { Name = "Original Name" };
        await _categoryService.AddCategoryAsync(category);

        category.Name = "Updated Name";
        await _categoryService.UpdateCategoryAsync(category);

        var updated = await _categoryService.GetCategoryByIdAsync(category.Id);
        Assert.IsNotNull(updated);
        Assert.AreEqual("Updated Name", updated!.Name);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanGetCategoriesByBookId()
    {
        var cat1 = new Category { Name = "Fiction" };
        var cat2 = new Category { Name = "Sci-Fi" };
        var cat3 = new Category { Name = "Non-Fiction" };

        await _categoryService.AddCategoryAsync(cat1);
        await _categoryService.AddCategoryAsync(cat2);
        await _categoryService.AddCategoryAsync(cat3);

        var categories = await _categoryService.GetAllCategoriesAsync();
        var book = new Book
        {
            Title = "Test Book",
            Author = "Test Author",
            Pages = 200,
            Price = 19.99M
        };

        await _bookService.AddBookAsync(book, new List<int> { cat1.Id, cat2.Id });
        var bookCategories = await _categoryService.GetCategoriesByBookIdAsync(book.Id);

        Assert.AreEqual(2, bookCategories.Count);
        Assert.IsTrue(bookCategories.Any(c => c.Name == "Fiction"));
        Assert.IsTrue(bookCategories.Any(c => c.Name == "Sci-Fi"));
        Assert.IsFalse(bookCategories.Any(c => c.Name == "Non-Fiction"));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanGetCategoryBooksCount()
    {
        var category = new Category { Name = "Test Category" };
        await _categoryService.AddCategoryAsync(category);

        var book1 = new Book { Title = "Book 1", Author = "Author 1", Pages = 100, Price = 9.99M };
        var book2 = new Book { Title = "Book 2", Author = "Author 2", Pages = 200, Price = 19.99M };

        await _bookService.AddBookAsync(book1, new List<int> { category.Id });
        await _bookService.AddBookAsync(book2, new List<int> { category.Id });

        var count = await _categoryService.GetCategoryBooksCountAsync(category.Id);

        Assert.AreEqual(2, count);
    }
    [TestMethod]
    [DoNotParallelize]
    public async Task CanAddAndGetCustomer()
    {
        var customer = new Customer
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "555-123-4567",
            RegistrationDate = DateTime.Now
        };

        await _customerService.AddCustomerAsync(customer);
        var retrievedCustomer = await _customerService.GetCustomerByIdAsync(customer.Id);

        Assert.IsNotNull(retrievedCustomer);
        Assert.AreEqual("John Doe", retrievedCustomer!.Name);
        Assert.AreEqual("john.doe@example.com", retrievedCustomer.Email);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanGetCustomerByEmail()
    {
        var customer = new Customer
        {
            Name = "Jane Smith",
            Email = "jane.smith@example.com",
            PhoneNumber = "555-987-6543",
            RegistrationDate = DateTime.Now
        };
        await _customerService.AddCustomerAsync(customer);

        var retrievedCustomer = await _customerService.GetCustomerByEmailAsync("jane.smith@example.com");
        Assert.IsNotNull(retrievedCustomer);
        Assert.AreEqual("Jane Smith", retrievedCustomer!.Name);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanUpdateCustomer()
    {
        var customer = new Customer
        {
            Name = "Original Name",
            Email = "original@example.com",
            PhoneNumber = "555-123-4567",
            RegistrationDate = DateTime.Now
        };
        await _customerService.AddCustomerAsync(customer);

        customer.Name = "Updated Name";
        customer.Email = "updated@example.com";
        customer.PhoneNumber = "555-987-6543";
        await _customerService.UpdateCustomerAsync(customer);

        var updated = await _customerService.GetCustomerByIdAsync(customer.Id);
        Assert.IsNotNull(updated);
        Assert.AreEqual("Updated Name", updated!.Name);
        Assert.AreEqual("updated@example.com", updated.Email);
        Assert.AreEqual("555-987-6543", updated.PhoneNumber);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanRemoveCustomer()
    {
        var customer = new Customer
        {
            Name = "To Be Removed",
            Email = "remove@example.com",
            PhoneNumber = "555-999-8888",
            RegistrationDate = DateTime.Now
        };
        await _customerService.AddCustomerAsync(customer);

        await _customerService.RemoveCustomerAsync(customer.Id);
        var retrievedCustomer = await _customerService.GetCustomerByIdAsync(customer.Id);

        Assert.IsNull(retrievedCustomer);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanCheckCustomerExists()
    {
        var customer = new Customer
        {
            Name = "Exist Test",
            Email = "exists@example.com",
            PhoneNumber = "555-111-2222",
            RegistrationDate = DateTime.Now
        };
        await _customerService.AddCustomerAsync(customer);

        var exists = await _customerService.CustomerExistsAsync("exists@example.com");
        var doesNotExist = await _customerService.CustomerExistsAsync("notexists@example.com");

        Assert.IsTrue(exists);
        Assert.IsFalse(doesNotExist);
    }
    [TestMethod]
    [DoNotParallelize]
    public async Task CanCreateAndGetOrder()
    {
        var customer = new Customer
        {
            Name = "Order Customer",
            Email = "order@example.com",
            PhoneNumber = "555-333-4444",
            RegistrationDate = DateTime.Now
        };
        await _customerService.AddCustomerAsync(customer);

        var book = new Book
        {
            Title = "Order Book",
            Author = "Order Author",
            Pages = 250,
            Price = 29.99M
        };
        await _bookService.AddBookAsync(book, new List<int>());

        var order = new Order
        {
            CustomerId = customer.Id,
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            TotalAmount = 29.99M
        };

        await _orderService.CreateOrderAsync(order);
        var retrievedOrder = await _orderService.GetOrderByIdAsync(order.Id);

        Assert.IsNotNull(retrievedOrder);
        Assert.AreEqual(customer.Id, retrievedOrder!.CustomerId);
        Assert.AreEqual(OrderStatus.Pending, retrievedOrder.Status);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanGetOrdersByCustomerId()
    {
        var customer = new Customer
        {
            Name = "Multi Order Customer",
            Email = "multiorder@example.com",
            PhoneNumber = "555-555-5555",
            RegistrationDate = DateTime.Now
        };
        await _customerService.AddCustomerAsync(customer);

        var order1 = new Order
        {
            CustomerId = customer.Id,
            OrderDate = DateTime.Now.AddDays(-1),
            Status = OrderStatus.Delivered,
            TotalAmount = 19.99M
        };

        var order2 = new Order
        {
            CustomerId = customer.Id,
            OrderDate = DateTime.Now,
            Status = OrderStatus.Processing,
            TotalAmount = 29.99M
        };

        await _orderService.CreateOrderAsync(order1);
        await _orderService.CreateOrderAsync(order2);

        var orders = await _orderService.GetOrdersByCustomerIdAsync(customer.Id);

        Assert.AreEqual(2, orders.Count);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanUpdateOrderStatus()
    {
        var customer = new Customer
        {
            Name = "Item Customer",
            Email = "item@example.com",
            PhoneNumber = "555-111-2222",
            RegistrationDate = DateTime.Now
        };
        await _customerService.AddCustomerAsync(customer);

        var order = new Order
        {
            CustomerId = customer.Id,
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            TotalAmount = 39.99M
        };
        await _orderService.CreateOrderAsync(order);

        await _orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Shipped);
        var updated = await _orderService.GetOrderByIdAsync(order.Id);

        Assert.IsNotNull(updated);
        Assert.AreEqual(OrderStatus.Shipped, updated!.Status);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanAddItemToOrder()
    {
        var customer = new Customer
        {
            Name = "Item Customer",
            Email = "item@example.com",
            PhoneNumber = "555-111-2222",
            RegistrationDate = DateTime.Now
        };
        await _customerService.AddCustomerAsync(customer);

        var book = new Book { Title = "Item Book", Author = "Item Author", Pages = 300, Price = 19.99M };
        await _bookService.AddBookAsync(book, new List<int>());

        var order = new Order
        {
            CustomerId = customer.Id,
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            TotalAmount = 0
        };
        await _orderService.CreateOrderAsync(order);

        await _orderService.AddItemToOrderAsync(order.Id, book.Id, 2);
        var updatedOrder = await _orderService.GetOrderByIdAsync(order.Id);

        Assert.IsNotNull(updatedOrder);
        Assert.AreEqual(1, updatedOrder!.OrderItems.Count);
        Assert.AreEqual(2, updatedOrder.OrderItems.First().Quantity);
        Assert.AreEqual(39.98M, updatedOrder.TotalAmount); // 19.99 * 2
    }
    [TestMethod]
    [DoNotParallelize]
    public async Task CanGetOrderItemsByOrderId()
    {
        var customer = new Customer
        {
            Name = "Item Customer",
            Email = "item@example.com",
            PhoneNumber = "555-111-2222",
            RegistrationDate = DateTime.Now
        };
        await _customerService.AddCustomerAsync(customer);

        var book1 = new Book { Title = "Book One", Author = "Author One", Pages = 100, Price = 10.99M };
        var book2 = new Book { Title = "Book Two", Author = "Author Two", Pages = 200, Price = 20.99M };
        await _bookService.AddBookAsync(book1, new List<int>());
        await _bookService.AddBookAsync(book2, new List<int>());

        var order = new Order
        {
            CustomerId = customer.Id,
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            TotalAmount = 0
        };
        await _orderService.CreateOrderAsync(order);

        await _orderService.AddItemToOrderAsync(order.Id, book1.Id, 1);
        await _orderService.AddItemToOrderAsync(order.Id, book2.Id, 2);
        var orderItems = await _orderItemService.GetOrderItemsByOrderIdAsync(order.Id);
        Assert.AreEqual(2, orderItems.Count);
        Assert.IsTrue(orderItems.Any(oi => oi.BookId == book1.Id && oi.Quantity == 1));
        Assert.IsTrue(orderItems.Any(oi => oi.BookId == book2.Id && oi.Quantity == 2));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanUpdateOrderItemQuantity()
    {
        var customer = new Customer
        {
            Name = "Item Customer",
            Email = "item@example.com",
            PhoneNumber = "555-111-2222",
            RegistrationDate = DateTime.Now
        };
        await _customerService.AddCustomerAsync(customer);

        var book = new Book { Title = "Quantity Book", Author = "Quantity Author", Pages = 150, Price = 15.99M };
        await _bookService.AddBookAsync(book, new List<int>());

        var order = new Order
        {
            CustomerId = customer.Id,
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            TotalAmount = 0
        };
        await _orderService.CreateOrderAsync(order);

        await _orderService.AddItemToOrderAsync(order.Id, book.Id, 1);
        var orderItems = await _orderItemService.GetOrderItemsByOrderIdAsync(order.Id);
        var orderItem = orderItems.First();

        await _orderItemService.UpdateOrderItemQuantityAsync(orderItem.Id, 3);

        var updatedItems = await _orderItemService.GetOrderItemsByOrderIdAsync(order.Id);
        var updatedItem = updatedItems.First();
        Assert.AreEqual(3, updatedItem.Quantity);

        // Check if order total was updated
        var updatedOrder = await _orderService.GetOrderByIdAsync(order.Id);
        Assert.AreEqual(47.97M, updatedOrder!.TotalAmount); // 15.99 * 3
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CanRemoveOrderItem()
    {
        var customer = new Customer
        {
            Name = "Item Customer",
            Email = "item@example.com",
            PhoneNumber = "555-111-2222",
            RegistrationDate = DateTime.Now
        };
        await _customerService.AddCustomerAsync(customer);

        var book = new Book { Title = "Remove Book", Author = "Remove Author", Pages = 175, Price = 17.99M };
        await _bookService.AddBookAsync(book, new List<int>());

        var order = new Order
        {
            CustomerId = customer.Id,
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            TotalAmount = 0
        };
        await _orderService.CreateOrderAsync(order);

        await _orderService.AddItemToOrderAsync(order.Id, book.Id, 2);
        var orderItems = await _orderItemService.GetOrderItemsByOrderIdAsync(order.Id);
        var orderItem = orderItems.First();
        await _orderItemService.RemoveOrderItemAsync(orderItem.Id);

        var updatedItems = await _orderItemService.GetOrderItemsByOrderIdAsync(order.Id);
        Assert.AreEqual(0, updatedItems.Count);

        // Check if order total was updated
        var updatedOrder = await _orderService.GetOrderByIdAsync(order.Id);
        Assert.AreEqual(0M, updatedOrder!.TotalAmount);
    }
}
