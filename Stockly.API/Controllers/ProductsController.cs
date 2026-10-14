using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stockly.API.Data;
using Stockly.API.DTOs;
using Stockly.API.Models;

namespace Stockly.API.Controllers;

[Route("api/[controller]")]
public class ProductsController : AuthorizedController
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    private static IQueryable<ProductResponseDto> Project(IQueryable<Product> query) =>
        query.Select(p => new ProductResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Sku = p.Sku,
            CostPrice = p.CostPrice,
            SalePrice = p.SalePrice,
            Quantity = p.Quantity,
            ReorderLevel = p.ReorderLevel,
            IsLowStock = p.Quantity <= p.ReorderLevel,
            CategoryId = p.CategoryId,
            CategoryName = p.Category!.Name
        });

    [HttpGet]
    public async Task<IActionResult> GetAll(string? search, int? categoryId, bool? lowStock)
    {
        var query = _db.Products.Where(p => p.UserId == CurrentUserId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(p => p.Name.Contains(s) || p.Sku.Contains(s));
        }

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (lowStock == true)
            query = query.Where(p => p.Quantity <= p.ReorderLevel);

        var result = await Project(query.OrderBy(p => p.Name)).ToListAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await Project(
            _db.Products.Where(p => p.Id == id && p.UserId == CurrentUserId))
            .FirstOrDefaultAsync();

        if (product is null)
            return NotFound(new { message = "Product not found" });

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductCreateDto dto)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.UserId == CurrentUserId);
        if (category is null)
            return BadRequest(new { message = "Category not found" });

        var sku = dto.Sku.Trim();
        var skuExists = await _db.Products
            .AnyAsync(p => p.UserId == CurrentUserId && p.Sku == sku);
        if (skuExists)
            return BadRequest(new { message = "SKU already exists" });

        var product = new Product
        {
            Name = dto.Name.Trim(),
            Sku = sku,
            CostPrice = dto.CostPrice,
            SalePrice = dto.SalePrice,
            Quantity = dto.InitialQuantity,
            ReorderLevel = dto.ReorderLevel,
            CategoryId = category.Id,
            UserId = CurrentUserId
        };

        if (dto.InitialQuantity > 0)
        {
            product.Movements.Add(new StockMovement
            {
                Type = MovementType.In,
                Quantity = dto.InitialQuantity,
                Date = DateTime.Now,
                Note = "Initial stock",
                UserId = CurrentUserId
            });
        }

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var response = await Project(_db.Products.Where(p => p.Id == product.Id)).FirstAsync();
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ProductDto dto)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == CurrentUserId);
        if (product is null)
            return NotFound(new { message = "Product not found" });

        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.UserId == CurrentUserId);
        if (category is null)
            return BadRequest(new { message = "Category not found" });

        var sku = dto.Sku.Trim();
        var skuExists = await _db.Products
            .AnyAsync(p => p.UserId == CurrentUserId && p.Sku == sku && p.Id != id);
        if (skuExists)
            return BadRequest(new { message = "SKU already exists" });

        product.Name = dto.Name.Trim();
        product.Sku = sku;
        product.CostPrice = dto.CostPrice;
        product.SalePrice = dto.SalePrice;
        product.ReorderLevel = dto.ReorderLevel;
        product.CategoryId = category.Id;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == CurrentUserId);
        if (product is null)
            return NotFound(new { message = "Product not found" });

        var movements = await _db.StockMovements
            .Where(m => m.ProductId == id && m.UserId == CurrentUserId)
            .ToListAsync();

        _db.StockMovements.RemoveRange(movements);
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}