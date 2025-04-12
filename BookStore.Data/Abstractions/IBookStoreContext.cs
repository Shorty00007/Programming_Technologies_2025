using BookStore.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Data.Abstractions;

public interface IBookStoreContext
{
    DbSet<Book> Books { get; }
    DbSet<Category> Categories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
