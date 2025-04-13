using BookStore.Data.Abstractions;
using BookStore.Data.Models;
using BookStore.Logic.Abstractions;
using Microsoft.EntityFrameworkCore;
namespace BookStore.Logic.Implementations;
public class CustomerService : ICustomerService
{
    private readonly IBookStoreContext _context;

    public CustomerService(IBookStoreContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        return await _context.Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Customer?> GetCustomerByEmailAsync(string email)
    {
        return await _context.Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        return await _context.Customers
            .Include(c => c.Orders)
            .ToListAsync();
    }

    public async Task AddCustomerAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCustomerAsync(Customer customer)
    {
        var existingCustomer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customer.Id);

        if (existingCustomer != null)
        {
            existingCustomer.Name = customer.Name;
            existingCustomer.Email = customer.Email;
            existingCustomer.PhoneNumber = customer.PhoneNumber;

            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveCustomerAsync(int id)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer != null)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Order>> GetCustomerOrdersAsync(int customerId)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Book)
            .Where(o => o.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<bool> CustomerExistsAsync(string email)
    {
        return await _context.Customers
            .AnyAsync(c => c.Email == email);
    }
}