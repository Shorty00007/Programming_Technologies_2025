namespace BookStore.Data.Models;
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public DateTime RegistrationDate { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
