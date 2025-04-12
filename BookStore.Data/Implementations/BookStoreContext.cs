using Microsoft.EntityFrameworkCore;
using BookStore.Data.Models;
using BookStore.Data.Abstractions;

namespace BookStore.Data.Implementations;

public class BookStoreContext : DbContext, IBookStoreContext
{
    public BookStoreContext(DbContextOptions<BookStoreContext> options)
        : base(options) { }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>()
            .HasMany(b => b.Categories)
            .WithMany(c => c.Books)
            .UsingEntity(j => j.ToTable("BookCategories"));

        base.OnModelCreating(modelBuilder);
    }
}
