using System.ComponentModel.DataAnnotations;

namespace FastenerFlow.Core.Models;

public class Order
{
    public int Id { get; set; }

    [Required]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending";

    public List<OrderItem> OrderItems { get; set; } = new();
}
