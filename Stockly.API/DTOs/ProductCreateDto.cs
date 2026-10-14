using System.ComponentModel.DataAnnotations;

namespace Stockly.API.DTOs;

public class ProductCreateDto : ProductDto
{
    [Range(0, 1000000)]
    public int InitialQuantity { get; set; }
}