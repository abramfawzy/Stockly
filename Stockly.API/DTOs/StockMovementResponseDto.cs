using Stockly.API.Models;

namespace Stockly.API.DTOs;

public class StockMovementResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public MovementType Type { get; set; }
    public int Quantity { get; set; }
    public DateTime Date { get; set; }
    public string? Note { get; set; }
}