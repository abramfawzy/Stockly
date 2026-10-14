using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stockly.API.Data;
using Stockly.API.DTOs;
using Stockly.API.Models;

namespace Stockly.API.Controllers;

[Route("api/[controller]")]
public class CategoriesController : AuthorizedController
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _db.Categories
            .Where(c => c.UserId == CurrentUserId)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                ProductsCount = c.Products.Count
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _db.Categories
            .Where(c => c.Id == id && c.UserId == CurrentUserId)
            .Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                ProductsCount = c.Products.Count
            })
            .FirstOrDefaultAsync();

        if (category is null)
            return NotFound(new { message = "Category not found" });

        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CategoryDto dto)
    {
        var name = dto.Name.Trim();

        var exists = await _db.Categories
            .AnyAsync(c => c.UserId == CurrentUserId && c.Name == name);
        if (exists)
            return BadRequest(new { message = "Category already exists" });

        var category = new Category { Name = name, UserId = CurrentUserId };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        var response = new CategoryResponseDto { Id = category.Id, Name = category.Name, ProductsCount = 0 };
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CategoryDto dto)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == CurrentUserId);
        if (category is null)
            return NotFound(new { message = "Category not found" });

        var name = dto.Name.Trim();

        var duplicate = await _db.Categories
            .AnyAsync(c => c.UserId == CurrentUserId && c.Name == name && c.Id != id);
        if (duplicate)
            return BadRequest(new { message = "Category already exists" });

        category.Name = name;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == CurrentUserId);
        if (category is null)
            return NotFound(new { message = "Category not found" });

        var hasProducts = await _db.Products.AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
            return BadRequest(new { message = "Cannot delete a category that has products" });

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}