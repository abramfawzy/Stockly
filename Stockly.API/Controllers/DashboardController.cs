using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stockly.API.Data;
using Stockly.API.DTOs;
using Stockly.API.Models;

namespace Stockly.API.Controllers;

[Route("api/[controller]")]
public class DashboardController : AuthorizedController
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/Dashboard/summary
    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var products = _db.Products.Where(p => p.UserId == CurrentUserId);

        var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var monthEnd = monthStart.AddMonths(1);

        var movements = _db.StockMovements
            .Where(m => m.UserId == CurrentUserId && m.Date >= monthStart && m.Date < monthEnd);

        var summary = new DashboardSummaryDto
        {
            TotalProducts = await products.CountAsync(),
            TotalUnits = await products.SumAsync(p => (int?)p.Quantity) ?? 0,
            StockValue = await products.SumAsync(p => (decimal?)(p.Quantity * p.CostPrice)) ?? 0,
            SaleValue = await products.SumAsync(p => (decimal?)(p.Quantity * p.SalePrice)) ?? 0,
            LowStockCount = await products.CountAsync(p => p.Quantity <= p.ReorderLevel),
            OutOfStockCount = await products.CountAsync(p => p.Quantity == 0),
            UnitsInThisMonth = await movements
                .Where(m => m.Type == MovementType.In)
                .SumAsync(m => (int?)m.Quantity) ?? 0,
            UnitsOutThisMonth = await movements
                .Where(m => m.Type == MovementType.Out)
                .SumAsync(m => (int?)m.Quantity) ?? 0
        };

        return Ok(summary);
    }
}