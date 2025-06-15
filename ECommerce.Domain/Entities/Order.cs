namespace ECommerce.Domain.Entities;
public class Order
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public List<Product> Products { get; set; } = new();
    public decimal TotalPrice => Products.Sum(p => p.Price);
    public string Status { get; set; } = "Pending";
    // Yeni eklenen alan:
    public string ReservationId { get; set; } = string.Empty;
}
