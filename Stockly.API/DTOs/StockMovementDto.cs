using System.ComponentModel.DataAnnotations;
using Stockly.API.Models;

namespace Stockly.API.DTOs;

public class StockMovementDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Required]
    public MovementType Type { get; set; }

    [Range(1, 1000000)]
    public int Quantity { get; set; }

    public DateTime? Date { get; set; }

    [MaxLength(200)]
    public string? Note { get; set; }
}