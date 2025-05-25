namespace BookStore.Data.Models;

public class ProcessState
{
    public int Id { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public int TotalBooksInStock { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
}
