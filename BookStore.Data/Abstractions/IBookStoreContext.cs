using BookStore.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Data.Abstractions;

public interface IBookStoreContext
{
    DbSet<Book> Books { get; }
    DbSet<Category> Categories { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}