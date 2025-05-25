using Microsoft.EntityFrameworkCore;
using BookStore.Data.Models;
using BookStore.Data.Abstractions;

namespace BookStore.Data.Implementations;

public class BookStoreContext : DbContext, IBookStoreContext
{
    public BookStoreContext(DbContextOptions<BookStoreContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<EventLog> EventLogs => Set<EventLog>();
    public DbSet<ProcessState> ProcessStates => Set<ProcessState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User-Order one-to-many
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Order-OrderItem one-to-many
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // OrderItem-Book one-to-many
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Book)
            .WithMany(b => b.OrderItems)
            .HasForeignKey(oi => oi.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        // EventLog optional User
        modelBuilder.Entity<EventLog>()
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        base.OnModelCreating(modelBuilder);
    }
}
