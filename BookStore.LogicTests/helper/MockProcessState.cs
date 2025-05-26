namespace BookStore.LogicTests.helper
{
    internal class MockProcessState
    {
        public int Id { get; set; }
        public DateTime RecordedAt { get; set; }
        public int TotalBooksInStock { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }

        public MockProcessState(int id, int stock, int orders, decimal revenue)
        {
            Id = id;
            RecordedAt = DateTime.UtcNow;
            TotalBooksInStock = stock;
            TotalOrders = orders;
            TotalRevenue = revenue;
        }
    }
}
