using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MiniSupermarket.API.Data;
using MiniSupermarket.API.dtos;
using System.Data;

namespace MiniSupermarket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly SupermarketDbContext _db;
        private readonly IConfiguration _configuration;

        public ReportsController(SupermarketDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
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
                .Include(o => o.Customer)
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
                    CustomerName = o.Customer != null ? o.Customer.CustomerName : "Khách mua tại quầy"
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

        // Chức năng: Thống kê doanh thu theo khoảng thời gian (Từ ngày - Đến ngày)
        // GET api/reports/date-range?fromDate=2026-09-01&toDate=2026-09-30
        [HttpGet("date-range")]
        public async Task<IActionResult> GetDateRangeReport([FromQuery] string? fromDate, [FromQuery] string? toDate)
        {
            try
            {
                DateTime start, end;

                if (!DateTime.TryParse(fromDate, out start))
                {
                    start = DateTime.Today.AddDays(-30);
                }

                if (!DateTime.TryParse(toDate, out end))
                {
                    end = DateTime.Today;
                }

                var startDate = start.Date;
                var endDate = end.Date.AddDays(1);

                if (startDate > endDate)
                {
                    return BadRequest(new { success = false, message = "Khoảng thời gian không hợp lệ: Từ ngày phải nhỏ hơn hoặc bằng Đến ngày." });
                }

                var orders = await _db.Orders
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate && o.Status == "PAID")
                    .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                    .Include(o => o.Customer)
                    .AsNoTracking()
                    .ToListAsync();

                int totalOrders = orders.Count;
                decimal totalRevenue = orders.Sum(o => o.Total);

                var dailyRevenue = orders
                    .GroupBy(o => o.CreatedAt.Date)
                    .Select(g => new
                    {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        OrderCount = g.Count(),
                        Revenue = g.Sum(o => o.Total)
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

                var topProducts = orders
                    .SelectMany(o => o.Items)
                    .GroupBy(i => new { i.ProductId, ProductName = i.Product != null ? i.Product.ProductName : $"SP #{i.ProductId}" })
                    .Select(g => new
                    {
                        g.Key.ProductName,
                        TotalQuantity = g.Sum(x => x.Quantity),
                        TotalAmount = g.Sum(x => x.LineTotal)
                    })
                    .OrderByDescending(x => x.TotalQuantity)
                    .Take(5)
                    .ToList();

                var recentOrders = orders
                    .OrderByDescending(o => o.CreatedAt)
                    .Select(o => new
                    {
                        o.OrderId,
                        o.OrderCode,
                        CustomerName = o.Customer != null ? o.Customer.CustomerName : "Khách mua tại quầy",
                        o.PaymentMethod,
                        o.Total,
                        o.CreatedAt
                    })
                    .ToList();

                return Ok(new
                {
                    success = true,
                    fromDate = startDate.ToString("yyyy-MM-dd"),
                    toDate = end.Date.ToString("yyyy-MM-dd"),
                    totalOrders = totalOrders,
                    totalRevenue = totalRevenue,
                    averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0,
                    dailyRevenue = dailyRevenue,
                    topProducts = topProducts,
                    recentOrders = recentOrders
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server: " + ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        // Chuc nang: Goi Stored Procedure sp_GetRevenueByCashier qua SqlDataReader
        [HttpGet("revenue-by-cashier")]
        [AllowAnonymous] // Cho phep kiem tra/truy cap truc tiep qua trinh duyet hoac Swagger/Postman
        public async Task<IActionResult> GetRevenueByCashier()
        {
            var reportList = new List<CashierRevenueDto>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection")!;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_GetRevenueByCashier", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    try
                    {
                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var item = new CashierRevenueDto
                                {
                                    CashierUsername = reader["CashierUsername"].ToString() ?? string.Empty,
                                    TotalOrders = Convert.ToInt32(reader["TotalOrders"]),
                                    TotalRevenue = Convert.ToDecimal(reader["TotalRevenue"]),
                                    TotalPointsAwarded = reader["TotalPointsAwarded"] != DBNull.Value
                                        ? Convert.ToInt32(reader["TotalPointsAwarded"])
                                        : 0
                                };

                                reportList.Add(item);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        return StatusCode(500, new
                        {
                            success = false,
                            message = "Lỗi thực thi Stored Procedure trên SQL Server: " + ex.Message
                        });
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, new
                        {
                            success = false,
                            message = "Lỗi xử lý dữ liệu: " + ex.Message
                        });
                    }
                }
            }

            return Ok(new
            {
                success = true,
                generatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                totalCashiers = reportList.Count,
                grandTotalRevenue = reportList.Sum(x => x.TotalRevenue),
                data = reportList
            });
        }

        // =======================================================================
        // Bài tập 3.1: Stored Procedure Báo cáo Doanh thu theo Khoảng ngày (Có tham số)
        // GET api/reports/revenue-by-date-range?fromDate=2026-09-01&toDate=2026-09-30
        // =======================================================================
        [HttpGet("revenue-by-date-range")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRevenueByDateRange([FromQuery] string? fromDate, [FromQuery] string? toDate)
        {
            DateTime start, end;
            if (!DateTime.TryParse(fromDate, out start))
            {
                start = DateTime.Today.AddDays(-30);
            }
            if (!DateTime.TryParse(toDate, out end))
            {
                end = DateTime.Today;
            }

            var startDateTime = start.Date;
            var endDateTime = end.Date.AddDays(1).AddTicks(-1); // đến hết 23:59:59.999 của toDate

            var reportList = new List<RevenueByDateRangeDto>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection")!;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_GetRevenueByDateRange", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@FromDate", startDateTime);
                    command.Parameters.AddWithValue("@ToDate", endDateTime);

                    try
                    {
                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var item = new RevenueByDateRangeDto
                                {
                                    CashierUsername = reader["CashierUsername"].ToString() ?? string.Empty,
                                    TotalOrders = Convert.ToInt32(reader["TotalOrders"]),
                                    TotalRevenue = Convert.ToDecimal(reader["TotalRevenue"])
                                };
                                reportList.Add(item);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        return StatusCode(500, new
                        {
                            success = false,
                            message = "Lỗi thực thi Stored Procedure trên SQL Server: " + ex.Message
                        });
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, new
                        {
                            success = false,
                            message = "Lỗi xử lý dữ liệu: " + ex.Message
                        });
                    }
                }
            }

            return Ok(new
            {
                success = true,
                fromDate = startDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                toDate = endDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                totalCashiers = reportList.Count,
                grandTotalRevenue = reportList.Sum(x => x.TotalRevenue),
                grandTotalOrders = reportList.Sum(x => x.TotalOrders),
                data = reportList
            });
        }

        // =======================================================================
        // Bài tập 3.2: Stored Procedure Top 5 Sản phẩm Bán chạy nhất
        // GET api/reports/top-5-selling-products
        // =======================================================================
        [HttpGet("top-5-selling-products")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTop5SellingProducts()
        {
            var topProducts = new List<TopSellingProductDto>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection")!;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_GetTop5SellingProducts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    try
                    {
                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var item = new TopSellingProductDto
                                {
                                    ProductId = Convert.ToInt32(reader["ProductId"]),
                                    ProductName = reader["ProductName"].ToString() ?? string.Empty,
                                    Unit = reader["Unit"].ToString() ?? string.Empty,
                                    Price = Convert.ToDecimal(reader["Price"]),
                                    TotalSoldQuantity = Convert.ToInt32(reader["TotalSoldQuantity"]),
                                    TotalRevenue = Convert.ToDecimal(reader["TotalRevenue"])
                                };
                                topProducts.Add(item);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        return StatusCode(500, new
                        {
                            success = false,
                            message = "Lỗi thực thi Stored Procedure trên SQL Server: " + ex.Message
                        });
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, new
                        {
                            success = false,
                            message = "Lỗi xử lý dữ liệu: " + ex.Message
                        });
                    }
                }
            }

            return Ok(new
            {
                success = true,
                generatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                totalCount = topProducts.Count,
                data = topProducts
            });
        }
    }
}