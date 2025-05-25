using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using BookStore.Data.Implementations;
using BookStore.Logic.Abstractions;
using BookStore.Logic.Implementations;
using System;
using System.Threading.Tasks;

namespace BookStore.IntegrationTests
{
    [TestClass]
    public class UserServiceIntegrationTests
    {
        private BookStoreContext _context = null!;
        private IUserService _userService = null!;
        private string _connectionString = null!;
        private string _dbName = null!;

        [TestInitialize]
        [DoNotParallelize]
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

            _userService = new UserService(_context);
        }

        [TestCleanup]
        [DoNotParallelize]
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
        public async Task CanRegisterAndLoginUser()
        {
            var username = "testuser";
            var password = "testpassword";

            var registered = await _userService.Register(username, password);

            Assert.IsNotNull(registered);
            Assert.AreEqual(username, registered.Username);

            var loggedIn = _userService.Login(username, password);

            Assert.IsNotNull(loggedIn);
            Assert.AreEqual(username, loggedIn!.Username);
        }

        [TestMethod]
        [DoNotParallelize]
        public async Task GetAllUsersAsync_ReturnsAllRegisteredUsers()
        {
            var username1 = "alice";
            var username2 = "bob";
            var password = "pass";

            await _userService.Register(username1, password);
            await _userService.Register(username2, password);

            var users = await _userService.GetAllUsersAsync();

            Assert.AreEqual(2, users.Count());
            Assert.IsTrue(users.Any(u => u.Username == username1));
            Assert.IsTrue(users.Any(u => u.Username == username2));
        }

    }
}
