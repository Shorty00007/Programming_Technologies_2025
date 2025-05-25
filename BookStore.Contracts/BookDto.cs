namespace BookStore.Contracts
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string ISBN { get; set; } = null!;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
