using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stockly.API.Data;
using Stockly.API.DTOs;
using Stockly.API.Models;

namespace Stockly.API.Controllers;

[Route("api/[controller]")]
public class StockMovementsController : AuthorizedController
{
    private readonly AppDbContext _db;

    public StockMovementsController(AppDbContext db)
    {
        _db = db;
    }

    private static IQueryable<StockMovementResponseDto> Project(IQueryable<StockMovement> query) =>
        query.Select(m => new StockMovementResponseDto
        {
            Id = m.Id,
            ProductId = m.ProductId,
            ProductName = m.Product!.Name,
            Sku = m.Product!.Sku,
            Type = m.Type,
            Quantity = m.Quantity,
            Date = m.Date,
            Note = m.Note
        });

    [HttpGet]
    public async Task<IActionResult> GetAll(int? productId, MovementType? type, DateTime? from, DateTime? to)
    {
        var query = _db.StockMovements.Where(m => m.UserId == CurrentUserId);

        if (productId.HasValue)
            query = query.Where(m => m.ProductId == productId.Value);

        if (type.HasValue)
            query = query.Where(m => m.Type == type.Value);

        if (from.HasValue)
            query = query.Where(m => m.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(m => m.Date < to.Value.Date.AddDays(1));

        var result = await Project(query
                .OrderByDescending(m => m.Date)
                .ThenByDescending(m => m.Id))
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var movement = await Project(
            _db.StockMovements.Where(m => m.Id == id && m.UserId == CurrentUserId))
            .FirstOrDefaultAsync();

        if (movement is null)
            return NotFound(new { message = "Movement not found" });

        return Ok(movement);
    }

    [HttpPost]
    public async Task<IActionResult> Create(StockMovementDto dto)
    {
        var productExists = await _db.Products
            .AnyAsync(p => p.Id == dto.ProductId && p.UserId == CurrentUserId);
        if (!productExists)
            return BadRequest(new { message = "Product not found" });

        await using var tx = await _db.Database.BeginTransactionAsync();

        int affected;
        if (dto.Type == MovementType.Out)
        {
            affected = await _db.Products
                .Where(p => p.Id == dto.ProductId && p.UserId == CurrentUserId && p.Quantity >= dto.Quantity)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Quantity, p => p.Quantity - dto.Quantity));

            if (affected == 0)
                return BadRequest(new { message = "Insufficient stock" });
        }
        else
        {
            affected = await _db.Products
                .Where(p => p.Id == dto.ProductId && p.UserId == CurrentUserId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Quantity, p => p.Quantity + dto.Quantity));
        }

        var movement = new StockMovement
        {
            ProductId = dto.ProductId,
            Type = dto.Type,
            Quantity = dto.Quantity,
            Date = dto.Date ?? DateTime.Now,
            Note = dto.Note,
            UserId = CurrentUserId
        };

        _db.StockMovements.Add(movement);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        var response = await Project(_db.StockMovements.Where(m => m.Id == movement.Id)).FirstAsync();
        return CreatedAtAction(nameof(GetById), new { id = movement.Id }, response);
    }
}