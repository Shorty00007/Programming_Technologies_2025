namespace BookStore.LogicTests.helper
{
    internal class MockEventLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; }
        public string Description { get; set; }
        public int? UserId { get; set; }

        public MockEventLog(int id, string type, string description, int? userId)
        {
            Id = id;
            Timestamp = DateTime.UtcNow;
            EventType = type;
            Description = description;
            UserId = userId;
        }
    }
}
