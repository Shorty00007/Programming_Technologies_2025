// helper/MockUser.cs
namespace BookStore.LogicTests.helper
{
    internal class MockUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }

        public MockUser(int id, string username, string passwordHash, string role)
        {
            Id = id;
            Username = username;
            PasswordHash = passwordHash;
            Role = role;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
