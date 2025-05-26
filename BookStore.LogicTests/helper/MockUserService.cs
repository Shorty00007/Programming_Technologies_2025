using BookStore.Contracts;
using BookStore.Logic.Abstractions;
namespace BookStore.LogicTests.helper;
public class MockUserService : IUserService
{
    private readonly List<UserDto> _users = new();
    private int _nextId = 1;

    public Task<UserDto> Register(string username, string password)
    {
        if (_users.Any(u => u.Username == username))
            throw new InvalidOperationException("Username already exists.");

        var user = new UserDto
        {
            Id = _nextId++,
            Username = username,
            Role = "Customer"
        };

        _users.Add(user);
        return Task.FromResult(user);
    }

    public UserDto? Login(string username, string password)
    {
        // W mocku hasło nie jest sprawdzane
        return _users.FirstOrDefault(u => u.Username == username);
    }

    public Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        return Task.FromResult<IEnumerable<UserDto>>(_users);
    }

    // Do testów — dodanie użytkownika
    public void AddUser(UserDto user)
    {
        if (!_users.Any(u => u.Id == user.Id))
        {
            _users.Add(user);
            _nextId = Math.Max(_nextId, user.Id + 1);
        }
    }

    // Do testów — dostęp do użytkowników
    public IEnumerable<UserDto> GetUsers()
    {
        return _users;
    }
}
