// helper/MockBook.cs
namespace BookStore.LogicTests.helper
{
    internal class MockBook
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }

        public MockBook(int id, string title, string author, string isbn, decimal price, int stock)
        {
            Id = id;
            Title = title;
            Author = author;
            ISBN = isbn;
            Price = price;
            Stock = stock;
        }
    }
}
