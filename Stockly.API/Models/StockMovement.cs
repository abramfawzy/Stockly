namespace Stockly.API.Models;

public class StockMovement
{
    public int Id { get; set; }
    public MovementType Type { get; set; }
    public int Quantity { get; set; }
    public DateTime Date { get; set; }
    public string? Note { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }
}