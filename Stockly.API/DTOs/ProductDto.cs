using System.ComponentModel.DataAnnotations;

namespace Stockly.API.DTOs;

public class ProductDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Sku { get; set; } = string.Empty;

    [Range(0, 999999999)]
    public decimal CostPrice { get; set; }

    [Range(0, 999999999)]
    public decimal SalePrice { get; set; }

    [Range(0, 1000000)]
    public int ReorderLevel { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}