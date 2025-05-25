using BookStore.Data;
using BookStore.Data.Models;
using BookStore.Contracts;
using BookStore.Logic.Abstractions;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using BookStore.Data.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Logic.Implementations;
public class UserService : IUserService
{
    private readonly IBookStoreContext _context;

    public UserService(IBookStoreContext context)
    {
        _context = context;
    }

    public async Task<UserDto> Register(string username, string password)
    {
        if (_context.Users.Any(u => u.Username == username))
            throw new InvalidOperationException("Username already exists.");

        var user = new User
        {
            Username = username,
            PasswordHash = password,
            Role = "Customer"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role
        };
    }

    public UserDto? Login(string username, string password)
    {
        var user = _context.Users
            .FirstOrDefault(u => u.Username == username && u.PasswordHash == password);

        if (user == null) return null;

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role
        };
    }
    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        return await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role
            })
            .ToListAsync();
    }

}
