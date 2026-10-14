namespace Stockly.API.DTOs;

public class DashboardSummaryDto
{
    public int TotalProducts { get; set; }
    public int TotalUnits { get; set; }

    public decimal StockValue { get; set; }

    public decimal SaleValue { get; set; }

    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }

    public int UnitsInThisMonth { get; set; }
    public int UnitsOutThisMonth { get; set; }
}