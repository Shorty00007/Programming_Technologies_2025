namespace BookStore.Contracts
{
    public class ProcessStateDto
    {
        public DateTime RecordedAt { get; set; }
        public int TotalBooksInStock { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
