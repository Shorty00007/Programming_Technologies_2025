namespace BookStore.Data.Models;

public class EventLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int? UserId { get; set; }
    public User? User { get; set; }
}
