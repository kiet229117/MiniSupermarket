using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSupermarket.API.Data;
using MiniSupermarket.API.Models;

namespace MiniSupermarket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly SupermarketDbContext _db;
        public ProductsController(SupermarketDbContext db) => _db = db;

        // GET api/products?keyword=gà&categoryId=4&brandId=8&supplierId=3&activeOnly=true
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAll(
            [FromQuery] string? keyword,
            [FromQuery] int? categoryId,
            [FromQuery] int? brandId,
            [FromQuery] int? supplierId,
            [FromQuery] bool activeOnly = false)
        {
            var q = _db.Products.AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Supplier)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(p => p.ProductName.Contains(keyword) || p.Barcode.Contains(keyword));
            if (categoryId != null) q = q.Where(p => p.CategoryId == categoryId);
            if (brandId != null) q = q.Where(p => p.BrandId == brandId);
            if (supplierId != null) q = q.Where(p => p.SupplierId == supplierId);
            if (activeOnly) q = q.Where(p => p.IsActive);

            return await q.OrderBy(p => p.ProductName).ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            var item = await _db.Products.AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.ProductId == id);
            return item == null ? NotFound() : item;
        }

        // GET api/products/barcode/8938501234561  (thu ngân quét mã vạch)
        [HttpGet("barcode/{barcode}")]
        [Authorize(Roles = "Admin, Cashier, Warehouse")]
        public async Task<ActionResult<Product>> GetByBarcode(string barcode)
        {
            var item = await _db.Products.AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Barcode == barcode && p.IsActive);
            return item == null ? NotFound("Không tìm thấy sản phẩm") : item;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Product>> Create(Product model)
        {
            var error = await ValidateAsync(model, null);
            if (error != null) return error;

            model.ProductId = 0;
            model.Category = null;      // tránh EF tạo mới danh mục từ JSON gửi lên
            model.Brand = null;
            model.Supplier = null;
            _db.Products.Add(model);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = model.ProductId }, model);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, Product model)
        {
            if (id != model.ProductId) return BadRequest("Id không khớp");

            var item = await _db.Products.FindAsync(id);
            if (item == null) return NotFound();

            var error = await ValidateAsync(model, id);
            if (error != null) return error;

            item.Barcode = model.Barcode;
            item.ProductName = model.ProductName;
            item.Unit = model.Unit;
            item.Price = model.Price;
            item.CostPrice = model.CostPrice;
            item.StockQuantity = model.StockQuantity;
            item.IsActive = model.IsActive;
            item.CategoryId = model.CategoryId;
            item.BrandId = model.BrandId;
            item.SupplierId = model.SupplierId;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Đã từng bán thì chỉ ngừng kinh doanh (IsActive = false), chưa bán thì xoá hẳn
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Products.FindAsync(id);
            if (item == null) return NotFound();

            if (await _db.OrderItems.AnyAsync(i => i.ProductId == id))
            {
                item.IsActive = false;
                await _db.SaveChangesAsync();
                return Ok("Sản phẩm đã có trong hoá đơn nên chỉ chuyển sang ngừng kinh doanh");
            }

            _db.Products.Remove(item);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Kiểm tra mã vạch trùng và khoá ngoại có tồn tại không
        private async Task<ActionResult?> ValidateAsync(Product model, int? currentId)
        {
            if (await _db.Products.AnyAsync(p => p.Barcode == model.Barcode && p.ProductId != currentId))
                return Conflict("Mã vạch đã tồn tại");
            if (!await _db.Categories.AnyAsync(c => c.CategoryId == model.CategoryId))
                return BadRequest("Nhóm hàng không tồn tại");
            if (model.BrandId > 0 && !await _db.Brands.AnyAsync(b => b.BrandId == model.BrandId))
                return BadRequest("Thương hiệu không tồn tại");
            if (model.SupplierId != null && !await _db.Suppliers.AnyAsync(s => s.SupplierId == model.SupplierId))
                return BadRequest("Nhà cung cấp không tồn tại");
            return null;
        }
    }
}