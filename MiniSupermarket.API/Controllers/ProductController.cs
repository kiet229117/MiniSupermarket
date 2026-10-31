using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MiniSupermarket.API.Data;
using MiniSupermarket.API.dtos;
using MiniSupermarket.API.Models;

namespace MiniSupermarket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/product")] // Hỗ trợ cả 2 đường dẫn /api/products và /api/product
    public class ProductsController : ControllerBase
    {
        private const int MaxPageSize = 100;
        private readonly SupermarketDbContext _db;

        public ProductsController(SupermarketDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Hàm chuyển đổi Entity Product sang ProductDto (Flattening)
        /// Tránh Circular Reference và bảo mật cấu trúc bảng CSDL
        /// </summary>
        private static ProductDto MapToDto(Product p)
        {
            return new ProductDto
            {
                ProductId = p.ProductId,
                Barcode = p.Barcode,
                ProductName = p.ProductName,
                Unit = p.Unit ?? string.Empty,
                Price = p.Price,
                CostPrice = p.CostPrice,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.CategoryName : string.Empty,
                BrandId = p.BrandId,
                BrandName = p.Brand != null ? p.Brand.BrandName : string.Empty,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier != null ? p.Supplier.SupplierName : null
            };
        }

        // =========================================================================
        // GET TẤT CẢ SẢN PHẨM + TÌM KIẾM LINQ + INCLUDE + ASNOTRACKING
        // GET: api/products?keyword=sua&categoryId=3&page=1&pageSize=10
        // =========================================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll(
            [FromQuery] string? keyword,
            [FromQuery] int? categoryId,
            [FromQuery] int? brandId,
            [FromQuery] int? supplierId,
            [FromQuery] bool activeOnly = false,
            [FromQuery] int? page = null,
            [FromQuery] int? pageSize = null)
        {
            //  Dùng .Include() nạp bảng liên quan và .AsNoTracking() tối ưu RAM
            var q = _db.Products.AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Supplier)
                .AsQueryable();

            // Tìm kiếm LINQ gần đúng bằng .Contains() và lọc theo nhóm hàng
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.Trim();
                q = q.Where(p => p.ProductName.Contains(kw) || p.Barcode.Contains(kw));
            }
            if (categoryId != null && categoryId > 0) q = q.Where(p => p.CategoryId == categoryId);
            if (brandId != null && brandId > 0) q = q.Where(p => p.BrandId == brandId);
            if (supplierId != null && supplierId > 0) q = q.Where(p => p.SupplierId == supplierId);
            if (activeOnly) q = q.Where(p => p.IsActive);

            q = q.OrderBy(p => p.ProductName).ThenBy(p => p.ProductId);

            // Tiêu chí 25: Hỗ trợ phân trang bằng .Skip() và .Take()
            if (page is > 0 && pageSize is > 0)
            {
                int size = Math.Min(pageSize.Value, MaxPageSize);
                q = q.Skip((page.Value - 1) * size).Take(size);
            }

            var list = await q.ToListAsync();
            return Ok(list.Select(MapToDto));
        }

        // =========================================================================
        //  PHÂN TRANG DỮ LIỆU PHÍA SERVER CHUYÊN DỤNG (SERVER-SIDE PAGINATION)
        // GET: api/products/paged?page=1&pageSize=10&keyword=...
        // =========================================================================
        [HttpGet("paged")]
        public async Task<ActionResult<object>> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] int? brandId = null,
            [FromQuery] int? supplierId = null,
            [FromQuery] bool activeOnly = false)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > MaxPageSize) pageSize = MaxPageSize;

            var q = _db.Products.AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Supplier)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.Trim();
                q = q.Where(p => p.ProductName.Contains(kw) || p.Barcode.Contains(kw));
            }
            if (categoryId != null && categoryId > 0) q = q.Where(p => p.CategoryId == categoryId);
            if (brandId != null && brandId > 0) q = q.Where(p => p.BrandId == brandId);
            if (supplierId != null && supplierId > 0) q = q.Where(p => p.SupplierId == supplierId);
            if (activeOnly) q = q.Where(p => p.IsActive);

            int totalCount = await q.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // Thuật toán LINQ phân trang Server-side
            var items = await q
                .OrderBy(p => p.ProductName)
                .ThenBy(p => p.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                totalCount,
                totalPages,
                page,
                pageSize,
                data = items.Select(MapToDto)
            });
        }

        // =========================================================================
        // LẤY CHI TIẾT SẢN PHẨM THEO ID
        // GET: api/products/5
        // =========================================================================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDto>> GetById(int id)
        {
            var p = await _db.Products.AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .Include(x => x.Supplier)
                .FirstOrDefaultAsync(x => x.ProductId == id);

            if (p == null) return NotFound("Không tìm thấy sản phẩm");
            return Ok(MapToDto(p));
        }

        // =========================================================================
        //  QUÉT MÃ VẠCH (BARCODE) BẰNG EF CORE - CÓ PHÂN QUYỀN
        // GET: api/products/barcode/8935001700018
        // =========================================================================
        [HttpGet("barcode/{barcode}")]
        [Authorize(Roles = "Admin, Cashier, Warehouse")]
        public async Task<ActionResult<ProductDto>> GetByBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return BadRequest("Mã vạch không được để trống!");

            var p = await _db.Products.AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .Include(x => x.Supplier)
                .FirstOrDefaultAsync(x => x.Barcode == barcode.Trim() && x.IsActive);

            if (p == null) return NotFound($"Không tìm thấy sản phẩm với mã vạch '{barcode}'");
            return Ok(MapToDto(p));
        }

        // =========================================================================
        // 5. TIÊU CHÍ 14, 20, 23, 30: CREATE - THÊM MỚI SẢN PHẨM (CHỈ ADMIN)
        // POST: api/products
        // =========================================================================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductDto>> Create([FromBody] Product model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kiểm tra ràng buộc hợp lệ
            var error = await ValidateProductAsync(model, null);
            if (error != null) return error;

            model.ProductId = 0;
            model.Unit = model.Unit ?? "cái";
            model.Category = null; // Tránh EF tự động tạo mới entity quan hệ
            model.Brand = null;
            model.Supplier = null;

            _db.Products.Add(model);
            await _db.SaveChangesAsync();

            // Nạp lại dữ liệu quan hệ để trả về DTO đầy đủ
            var created = await _db.Products.AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Supplier)
                .FirstAsync(p => p.ProductId == model.ProductId);

            return CreatedAtAction(nameof(GetById), new { id = model.ProductId }, MapToDto(created));
        }

        // =========================================================================
        // 6. TIÊU CHÍ 14, 23, 30: UPDATE - CẬP NHẬT SẢN PHẨM (CHỈ ADMIN)
        // PUT: api/products/5
        // =========================================================================
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Product model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var item = await _db.Products.FindAsync(id);
            if (item == null) return NotFound("Không tìm thấy sản phẩm cần cập nhật!");

            var error = await ValidateProductAsync(model, id);
            if (error != null) return error;

            item.Barcode = model.Barcode.Trim();
            item.ProductName = model.ProductName.Trim();
            item.Unit = model.Unit ?? "cái";
            item.Price = model.Price;
            item.CostPrice = model.CostPrice;
            item.StockQuantity = model.StockQuantity;
            item.IsActive = model.IsActive;
            item.CategoryId = model.CategoryId;
            item.BrandId = model.BrandId;
            item.SupplierId = model.SupplierId;

            await _db.SaveChangesAsync();
            return NoContent(); // Chuẩn RESTful trả 204
        }

        // =========================================================================
        // 7. TIÊU CHÍ 14, 23, 30: DELETE - XÓA SẢN PHẨM (CHỈ ADMIN)
        // DELETE: api/products/5
        // =========================================================================
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Products.FindAsync(id);
            if (item == null) return NotFound("Không tìm thấy sản phẩm cần xóa!");

            // Kiểm tra ràng buộc: Nếu đã từng bán trong đơn hàng -> Chuyển ngừng kinh doanh (Soft Delete)
            bool hasOrders = await _db.OrderItems.AnyAsync(i => i.ProductId == id);
            if (hasOrders)
            {
                item.IsActive = false;
                await _db.SaveChangesAsync();
                return Ok(new { success = true, message = "Sản phẩm đã phát sinh hóa đơn nên hệ thống đã chuyển sang trạng thái [Ngừng kinh doanh] để bảo toàn dữ liệu lịch sử!" });
            }

            // Chưa phát sinh hóa đơn -> Xóa hẳn khỏi CSDL
            _db.Products.Remove(item);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // =========================================================================
        // 8. TIÊU CHÍ 32, 33: TRA CỨU BARCODE BẰNG ADO.NET THUẦN TÚY (HIỆU NĂNG TỨC THÌ)
        // GET: api/products/barcode-ado/8935001700018
        // =========================================================================
        [HttpGet("barcode-ado/{barcode}")]
        [Authorize(Roles = "Admin, Cashier, Warehouse")]
        public async Task<ActionResult<ProductDto>> GetByBarcodeAdo(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return BadRequest("Mã vạch không được để trống!");

            string connectionString = _db.Database.GetConnectionString()
                ?? throw new InvalidOperationException("Thiếu connection string CSDL!");

            const string sql = @"
                SELECT TOP 1
                    p.ProductId, p.Barcode, p.ProductName, p.Unit, p.Price, p.CostPrice,
                    p.StockQuantity, p.IsActive, p.CategoryId, p.BrandId, p.SupplierId,
                    c.CategoryName, b.BrandName, s.SupplierName
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
                LEFT JOIN Brands b ON p.BrandId = b.BrandId
                LEFT JOIN Suppliers s ON p.SupplierId = s.SupplierId
                WHERE p.Barcode = @Barcode AND p.IsActive = 1";

            // Dùng SqlConnection, SqlCommand
            await using var connection = new SqlConnection(connectionString);
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("@Barcode", SqlDbType.NVarChar, 50) { Value = barcode.Trim() });

            await connection.OpenAsync();

            // Đọc dữ liệu bằng SqlDataReader 
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow);

            if (!await reader.ReadAsync())
                return NotFound($"Không tìm thấy sản phẩm với mã vạch '{barcode}' (ADO.NET)");

            string Str(string col) => reader.IsDBNull(reader.GetOrdinal(col)) ? string.Empty : reader.GetString(reader.GetOrdinal(col));
            int Int(string col) => reader.IsDBNull(reader.GetOrdinal(col)) ? 0 : reader.GetInt32(reader.GetOrdinal(col));

            var dto = new ProductDto
            {
                ProductId = Int("ProductId"),
                Barcode = Str("Barcode"),
                ProductName = Str("ProductName"),
                Unit = Str("Unit"),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                CostPrice = reader.GetDecimal(reader.GetOrdinal("CostPrice")),
                StockQuantity = Int("StockQuantity"),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CategoryId = Int("CategoryId"),
                BrandId = Int("BrandId"),
                SupplierId = reader.IsDBNull(reader.GetOrdinal("SupplierId")) ? null : reader.GetInt32(reader.GetOrdinal("SupplierId")),
                CategoryName = Str("CategoryName"),
                BrandName = Str("BrandName"),
                SupplierName = Str("SupplierName")
            };

            return Ok(dto);
        }

        /// <summary>
        /// Hàm kiểm tra logic nghiệp vụ chống trùng mã vạch và kiểm tra khóa ngoại
        /// </summary>
        private async Task<ActionResult?> ValidateProductAsync(Product model, int? currentId)
        {
            if (string.IsNullOrWhiteSpace(model.Barcode))
                return BadRequest("Mã vạch sản phẩm không được để trống!");

            if (string.IsNullOrWhiteSpace(model.ProductName))
                return BadRequest("Tên sản phẩm không được để trống!");

            if (model.Price <= 0)
                return BadRequest("Giá bán sản phẩm phải lớn hơn 0!");

            // Chống trùng mã vạch
            bool isBarcodeExist = await _db.Products.AnyAsync(p => p.Barcode == model.Barcode.Trim() && p.ProductId != currentId);
            if (isBarcodeExist)
                return Conflict("Mã vạch này đã tồn tại trên hệ thống!");

            // Kiểm tra nhóm hàng tồn tại
            bool isCatExist = await _db.Categories.AnyAsync(c => c.CategoryId == model.CategoryId);
            if (!isCatExist)
                return BadRequest("Nhóm hàng được chọn không tồn tại trong CSDL!");

            // Kiểm tra thương hiệu tồn tại
            if (model.BrandId > 0)
            {
                bool isBrandExist = await _db.Brands.AnyAsync(b => b.BrandId == model.BrandId);
                if (!isBrandExist)
                    return BadRequest("Thương hiệu được chọn không tồn tại trong CSDL!");
            }

            // Kiểm tra nhà cung cấp tồn tại
            if (model.SupplierId != null && model.SupplierId > 0)
            {
                bool isSupplierExist = await _db.Suppliers.AnyAsync(s => s.SupplierId == model.SupplierId);
                if (!isSupplierExist)
                    return BadRequest("Nhà cung cấp được chọn không tồn tại trong CSDL!");
            }

            return null;
        }
    }
}
