namespace BookStore.Contracts
{
    public class EventLogDto
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int? UserId { get; set; }
    }
}
