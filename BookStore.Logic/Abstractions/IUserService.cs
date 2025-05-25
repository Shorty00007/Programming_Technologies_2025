using BookStore.Contracts;
using System.Threading.Tasks;

namespace BookStore.Logic.Abstractions;

public interface IUserService
{
    Task<UserDto> Register(string username, string password);
    UserDto? Login(string username, string password);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
}