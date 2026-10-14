using System.ComponentModel.DataAnnotations;

namespace Stockly.API.DTOs;

public class CategoryDto
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}