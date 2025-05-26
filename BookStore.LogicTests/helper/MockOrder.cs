// helper/MockOrder.cs
namespace BookStore.LogicTests.helper
{
    internal class MockOrder
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }

        public MockOrder(int id, int userId, decimal total)
        {
            Id = id;
            UserId = userId;
            TotalAmount = total;
            OrderDate = DateTime.UtcNow;
        }
    }
}
