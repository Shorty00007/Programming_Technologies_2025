namespace BookStore.LogicTests.helper
{
    internal class MockOrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public MockOrderItem(int id, int orderId, int bookId, int quantity, decimal unitPrice)
        {
            Id = id;
            OrderId = orderId;
            BookId = bookId;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }
    }
}
