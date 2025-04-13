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
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Book-Category many-to-many relationship
        modelBuilder.Entity<Book>()
            .HasMany(b => b.Categories)
            .WithMany(c => c.Books)
            .UsingEntity(j => j.ToTable("BookCategories"));

        // Customer-Order one-to-many relationship
        modelBuilder.Entity<Order>()
         .HasOne(o => o.Customer)
         .WithMany(c => c.Orders)
         .HasForeignKey(o => o.CustomerId)
         .OnDelete(DeleteBehavior.Cascade);

        // Order-OrderItem one-to-many relationship
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // OrderItem-Book one-to-many relationship
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Book)
            .WithMany()
            .HasForeignKey(oi => oi.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
}
