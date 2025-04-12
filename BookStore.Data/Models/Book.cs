namespace BookStore.Data.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Author { get; set; } = null!;
    public int Pages { get; set; }
    public decimal Price { get; set; }

    public ICollection<Category> Categories { get; set; } = new List<Category>();
}
