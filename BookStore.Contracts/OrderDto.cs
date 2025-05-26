namespace BookStore.Contracts
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }

        public List<OrderItemDto> Items { get; set; } = new();
        public string Username { get; set; } = string.Empty;
    }
}
