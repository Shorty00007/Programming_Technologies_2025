using BookStore.Data.Models;
namespace BookStore.Logic.Abstractions;
public interface ICustomerService
{
    Task<Customer?> GetCustomerByIdAsync(int id);
    Task<Customer?> GetCustomerByEmailAsync(string email);
    Task<List<Customer>> GetAllCustomersAsync();
    Task AddCustomerAsync(Customer customer);
    Task UpdateCustomerAsync(Customer customer);
    Task RemoveCustomerAsync(int id);
    Task<List<Order>> GetCustomerOrdersAsync(int customerId);
    Task<bool> CustomerExistsAsync(string email);
}
