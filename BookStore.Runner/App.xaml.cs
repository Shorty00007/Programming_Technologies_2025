using BookStore.Data.Implementations;
using BookStore.Logic.Implementations;
using BookStore.Logic.Abstractions;
using BookStore.Presentation.ViewModels;
using BookStore.Presentation.Views;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Windows;

namespace BookStore.Runner
{
    public partial class App : Application
    {
        private string _connectionString = null!;
        private string _dbName = null!;

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            _connectionString = config.GetConnectionString("DefaultConnection")!;
            _dbName = new SqlConnectionStringBuilder(_connectionString).InitialCatalog!;

            RecreateDatabase();

            var options = new DbContextOptionsBuilder<BookStoreContext>()
                .UseSqlServer(_connectionString)
                .Options;

            var context = new BookStoreContext(options);

            var userService = new UserService(context);
            var bookService = new BookService(context);
            var orderService = new OrderService(context);
            var eventLogService = new EventLogService(context);
            var processStateService = new ProcessStateService(context);

            var mainViewModel = new MainWindowViewModel(
                userService,
                bookService,
                orderService,
                eventLogService,
                processStateService);
            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
            mainWindow.Show();
        }

        private async void RecreateDatabase()
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
                MessageBox.Show($"Failed to drop DB: {ex.Message}");
            }

            var options = new DbContextOptionsBuilder<BookStoreContext>()
                .UseSqlServer(_connectionString)
                .Options;

            using var tempContext = new BookStoreContext(options);
            tempContext.Database.EnsureCreated();

            IUserService userService = new UserService(tempContext);
            IBookService bookService = new BookService(tempContext);
            IOrderService orderService = new OrderService(tempContext);

            var allUsers = await userService.GetAllUsersAsync();
            var admin = allUsers.FirstOrDefault(u => u.Username == "admin");
            if (admin == null)
            {
                var createdAdmin = await userService.Register("admin", "admin123");
                var adminEntity = await tempContext.Users.FindAsync(createdAdmin.Id);
                if (adminEntity != null)
                {
                    adminEntity.Role = "Admin";
                    await tempContext.SaveChangesAsync();
                }
            }

            var customer = allUsers.FirstOrDefault(u => u.Username == "customer1");
            if (customer == null)
            {
                customer = await userService.Register("customer1", "pass123");
            }
            else
            {
                var login = userService.Login("customer1", "pass123");
                if (login != null) customer = login;
            }

            var existingBooks = (await bookService.GetAllBooksAsync()).ToList();
            if (!existingBooks.Any())
            {
                await bookService.AddBookAsync("Clean Code", "Robert C. Martin", "9780132350884", 45.00m, 10);
                await bookService.AddBookAsync("The Pragmatic Programmer", "Andrew Hunt", "9780201616224", 50.00m, 8);
                await bookService.AddBookAsync("Domain-Driven Design", "Eric Evans", "9780321125217", 60.00m, 5);
                existingBooks = (await bookService.GetAllBooksAsync()).ToList(); // Refresh list
            }

            if (!tempContext.Orders.Any())
            {
                var book1 = existingBooks.First(b => b.Title == "Clean Code");
                var book2 = existingBooks.First(b => b.Title == "The Pragmatic Programmer");

                await orderService.PlaceOrderAsync(customer.Id, new List<(int bookId, int quantity)>
                {
                    (book1.Id, 1),
                    (book2.Id, 2)
                });
            }

        }
    }
}
