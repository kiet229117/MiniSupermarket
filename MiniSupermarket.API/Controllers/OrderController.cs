using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSupermarket.API.Data;
using MiniSupermarket.API.Dtos;
using MiniSupermarket.API.Models;

namespace MiniSupermarket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private static readonly string[] AllowedPayments = { "CASH", "CARD", "MOMO", "BANK_TRANSFER" };
        private const decimal PointUnit = 10000m;   // cứ 10.000đ thanh toán = 1 điểm thưởng (tuỳ chỉnh)

        private readonly SupermarketDbContext _db;
        public OrdersController(SupermarketDbContext db) => _db = db;

        // GET api/orders?status=PAID&customerId=1&from=2026-09-01&to=2026-09-30
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status,
            [FromQuery] int? customerId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var q = _db.Orders.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(o => o.Status == status);
            if (customerId != null) q = q.Where(o => o.CustomerId == customerId);
            if (from != null) q = q.Where(o => o.CreatedAt >= from);
            if (to != null) q = q.Where(o => o.CreatedAt < to.Value.AddDays(1));   // gồm cả ngày "to"

            var list = await q.OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderCode,
                    o.CreatedAt,
                    o.Status,
                    o.PaymentMethod,
                    o.Subtotal,
                    o.Discount,
                    o.Total,
                    CustomerName = o.Customer != null ? o.Customer.CustomerName : null,
                    CashierName = o.Cashier != null ? o.Cashier.FullName : null,
                    ItemCount = o.Items.Count
                })
                .ToListAsync();
            return Ok(list);
        }

        // GET api/orders/5  (kèm chi tiết từng dòng hàng)
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _db.Orders.AsNoTracking()
                .Where(o => o.OrderId == id)
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderCode,
                    o.CreatedAt,
                    o.Status,
                    o.PaymentMethod,
                    o.Subtotal,
                    o.Discount,
                    o.Total,
                    o.CustomerId,
                    o.CashierId,
                    CustomerName = o.Customer != null ? o.Customer.CustomerName : null,
                    CashierName = o.Cashier != null ? o.Cashier.FullName : null,
                    Items = o.Items.Select(i => new
                    {
                        i.OrderItemId,
                        i.ProductId,
                        ProductName = i.Product != null ? i.Product.ProductName : null,
                        i.Quantity,
                        i.UnitPrice,
                        i.LineTotal
                    })
                })
                .FirstOrDefaultAsync();
            return order == null ? NotFound() : Ok(order);
        }

        // POST api/orders
        [HttpPost]
        [Authorize(Roles = "Admin, Cashier")]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            if (!AllowedPayments.Contains(dto.PaymentMethod))
                return BadRequest("Phương thức thanh toán không hợp lệ");

            if (!await _db.Users.AnyAsync(u => u.UserId == dto.CashierId && u.IsActive))
                return BadRequest("Thu ngân không tồn tại hoặc đã bị khoá");

            Customer? customer = null;
            if (dto.CustomerId != null)
            {
                customer = await _db.Customers.FindAsync(dto.CustomerId);
                if (customer == null) return BadRequest("Khách hàng không tồn tại");
            }

            // Gộp các dòng trùng sản phẩm (quét 2 lần cùng 1 mã)
            var lines = dto.Items
                .GroupBy(i => i.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToList();

            var ids = lines.Select(l => l.ProductId).ToList();
            var products = await _db.Products.Where(p => ids.Contains(p.ProductId))
                                             .ToDictionaryAsync(p => p.ProductId);

            var items = new List<OrderItem>();
            foreach (var l in lines)
            {
                if (!products.TryGetValue(l.ProductId, out var p) || !p.IsActive)
                    return BadRequest($"Sản phẩm #{l.ProductId} không tồn tại hoặc đã ngừng kinh doanh");
                if (p.StockQuantity < l.Quantity)
                    return BadRequest($"'{p.ProductName}' chỉ còn {p.StockQuantity} trong kho");

                items.Add(new OrderItem
                {
                    ProductId = p.ProductId,
                    Quantity = l.Quantity,
                    UnitPrice = p.Price,                 // chốt giá tại thời điểm bán
                    LineTotal = p.Price * l.Quantity
                });
                p.StockQuantity -= l.Quantity;           // trừ kho
            }

            var subtotal = items.Sum(i => i.LineTotal);
            if (dto.Discount > subtotal)
                return BadRequest("Giảm giá không được lớn hơn tổng tiền hàng");

            var order = new Order
            {
                OrderCode = Guid.NewGuid().ToString("N")[..20],   // mã tạm, đổi sau khi có OrderId
                CustomerId = dto.CustomerId,
                CashierId = dto.CashierId,
                Subtotal = subtotal,
                Discount = dto.Discount,
                Total = subtotal - dto.Discount,
                PaymentMethod = dto.PaymentMethod,
                Status = "PAID",
                CreatedAt = DateTime.Now,
                Items = items
            };

            if (customer != null)
                customer.RewardPoints += (int)(order.Total / PointUnit);   // cộng điểm thưởng

            // Transaction: hoá đơn, trừ kho, cộng điểm cùng thành công hoặc cùng thất bại
            await using var tx = await _db.Database.BeginTransactionAsync();
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();                 // sinh OrderId

            order.OrderCode = $"HD{order.OrderId:D4}";    // HD0013, HD0014...
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return CreatedAtAction(nameof(GetById), new { id = order.OrderId },
                new { order.OrderId, order.OrderCode, order.Subtotal, order.Discount, order.Total });
        }

        // POST api/orders/checkout (phục vụ trực tiếp cho màn hình POS WinForms)
        [HttpPost("checkout")]
        [Authorize(Roles = "Admin, Cashier")]
        public async Task<IActionResult> Checkout(PosCheckoutDto dto)
        {
            if (dto.Items == null || dto.Items.Count == 0)
                return BadRequest("Giỏ hàng không có sản phẩm");

            // Xác định CashierId
            int cashierId = 4; // Mặc định thu ngân 1 nếu không tìm thấy
            if (!string.IsNullOrWhiteSpace(dto.CashierUsername))
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.CashierUsername);
                if (user != null) cashierId = user.UserId;
            }

            // Tìm Customer theo SĐT nếu có
            Customer? customer = null;
            if (!string.IsNullOrWhiteSpace(dto.CustomerPhone))
            {
                customer = await _db.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == dto.CustomerPhone.Trim());
            }

            var lines = dto.Items
                .GroupBy(i => i.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToList();

            var ids = lines.Select(l => l.ProductId).ToList();
            var products = await _db.Products.Where(p => ids.Contains(p.ProductId))
                                             .ToDictionaryAsync(p => p.ProductId);

            var items = new List<OrderItem>();
            foreach (var l in lines)
            {
                if (!products.TryGetValue(l.ProductId, out var p) || !p.IsActive)
                    return BadRequest($"Sản phẩm #{l.ProductId} không tồn tại hoặc đã ngừng kinh doanh");
                if (p.StockQuantity < l.Quantity)
                    return BadRequest($"'{p.ProductName}' chỉ còn {p.StockQuantity} trong kho");

                items.Add(new OrderItem
                {
                    ProductId = p.ProductId,
                    Quantity = l.Quantity,
                    UnitPrice = p.Price,
                    LineTotal = p.Price * l.Quantity
                });
                p.StockQuantity -= l.Quantity;
            }

            var subtotal = items.Sum(i => i.LineTotal);
            var discount = Math.Max(0, dto.Discount);
            if (discount > subtotal) discount = subtotal;

            var order = new Order
            {
                OrderCode = Guid.NewGuid().ToString("N")[..20],
                CustomerId = customer?.CustomerId,
                CashierId = cashierId,
                Subtotal = subtotal,
                Discount = discount,
                Total = subtotal - discount,
                PaymentMethod = string.IsNullOrWhiteSpace(dto.PaymentMethod) ? "CASH" : dto.PaymentMethod.ToUpper(),
                Status = "PAID",
                CreatedAt = DateTime.Now,
                Items = items
            };

            if (customer != null)
                customer.RewardPoints += (int)(order.Total / PointUnit);

            await using var tx = await _db.Database.BeginTransactionAsync();
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            order.OrderCode = $"HD{order.OrderId:D4}";
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(new
            {
                order.OrderId,
                order.OrderCode,
                order.Subtotal,
                order.Discount,
                order.Total,
                CustomerName = customer?.CustomerName ?? "Khách vãng lai"
            });
        }

        // PUT api/orders/5/cancel : huỷ hoá đơn, hoàn kho, thu hồi điểm thưởng
        [HttpPut("{id:int}/cancel")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _db.Orders.Include(o => o.Items)
                                        .FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null) return NotFound();
            if (order.Status != "PAID")
                return BadRequest("Chỉ huỷ được hoá đơn đang ở trạng thái PAID");

            var ids = order.Items.Select(i => i.ProductId).ToList();
            var products = await _db.Products.Where(p => ids.Contains(p.ProductId))
                                             .ToDictionaryAsync(p => p.ProductId);
            foreach (var i in order.Items)
                if (products.TryGetValue(i.ProductId, out var p))
                    p.StockQuantity += i.Quantity;        // hoàn kho

            if (order.CustomerId != null)
            {
                var customer = await _db.Customers.FindAsync(order.CustomerId);
                if (customer != null)
                    customer.RewardPoints = Math.Max(0, customer.RewardPoints - (int)(order.Total / PointUnit));
            }

            order.Status = "CANCELLED";
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}