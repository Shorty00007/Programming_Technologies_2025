using BookStore.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Data.Abstractions;

public interface IBookStoreContext
{
    DbSet<User> Users { get; }
    DbSet<Book> Books { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<EventLog> EventLogs { get; }
    DbSet<ProcessState> ProcessStates { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
