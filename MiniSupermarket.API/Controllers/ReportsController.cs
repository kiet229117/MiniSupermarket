using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSupermarket.API.Data;

namespace MiniSupermarket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly SupermarketDbContext _db;

        public ReportsController(SupermarketDbContext db)
        {
            _db = db;
        }

        // GET api/reports/daily?date=2026-09-05
        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyReport([FromQuery] string? date)
        {
            DateTime targetDate;
            if (!DateTime.TryParse(date, out targetDate))
            {
                targetDate = DateTime.Today;
            }

            var startDate = targetDate.Date;
            var endDate = startDate.AddDays(1);

            // Các đơn hàng đã thanh toán trong ngày
            var orders = await _db.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate && o.Status == "PAID")
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .AsNoTracking()
                .ToListAsync();

            int totalOrders = orders.Count;
            decimal totalRevenue = orders.Sum(o => o.Total);

            // Tìm mặt hàng bán chạy nhất
            var bestSellerGroup = orders
                .SelectMany(o => o.Items)
                .GroupBy(i => new { i.ProductId, ProductName = i.Product != null ? i.Product.ProductName : $"SP #{i.ProductId}" })
                .Select(g => new
                {
                    g.Key.ProductName,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalAmount = g.Sum(x => x.LineTotal)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .FirstOrDefault();

            string bestSeller = bestSellerGroup != null
                ? $"{bestSellerGroup.ProductName} ({bestSellerGroup.TotalQuantity} sp)"
                : "Chưa có";

            var recentOrders = orders.OrderByDescending(o => o.CreatedAt)
                .Take(20)
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderCode,
                    o.CreatedAt,
                    o.Total,
                    o.PaymentMethod,
                    CustomerName = o.Customer != null ? o.Customer.CustomerName : "Khách vãng lai"
                })
                .ToList();

            return Ok(new
            {
                Date = startDate.ToString("yyyy-MM-dd"),
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                BestSeller = bestSeller,
                RecentOrders = recentOrders
            });
        }
    }
}

