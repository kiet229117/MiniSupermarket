<<<<<<< HEAD
﻿using Microsoft.AspNetCore.Mvc;
=======
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSupermarket.API.Data;
>>>>>>> buoi-5
using MiniSupermarket.API.Models;

namespace MiniSupermarket.API.Controllers
{
<<<<<<< HEAD
    [Route("api/[controller]")] // Định tuyến cơ sở: /api/categories
    [ApiController]
    public class CategoriesController : ControllerBase
    {

        // Dữ liệu mẫu lưu tạm trên bộ nhớ RAM (In-Memory) phục vụ kiểm thử khi chưa có Database
        private static readonly List<Category> _categories = new() {
            new Category { CategoryId = 1, CategoryName = "Bánh kẹo & Đồ ăn vặt", Description = "Snack, bánh quy, kẹo dẻo" },
            new Category { CategoryId = 2, CategoryName = "Nước giải khát & Trà", Description = "Nước ngọt, nước khoáng, trà" },
            new Category { CategoryId = 3, CategoryName = "Sữa & Sản phẩm từ sữa", Description = "Sữa tươi, sữa chua, phô mai" },
            new Category { CategoryId = 4, CategoryName = "Mì gói & Thực phẩm ăn liền", Description = "Mì ăn liền, phở khô, cháo gói" },
            new Category { CategoryId = 5, CategoryName = "Gia vị & Dầu ăn", Description = "Nước mắm, hạt nêm, dầu thực vật" }
        };

        // 1. READ: Lấy toàn bộ danh sách nhóm hàng (GET /api/categories)
        [HttpGet]
        public IActionResult GetAll()
        {
            // Trả về mã 200 OK kèm theo danh sách JSON
            return Ok(_categories);
        }

        // 2. READ: Lấy chi tiết một nhóm hàng theo ID (GET /api/categories/{id})
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var cat = _categories.FirstOrDefault(c => c.CategoryId == id);
            if (cat == null)
            {
                // Trả về mã lỗi 404 nếu không tìm thấy ID tương ứng
                return NotFound(new { message = "Không tìm thấy nhóm hàng!" });
            }
            return Ok(cat);
        }

        // 3. SEARCH: Tìm kiếm nhóm hàng theo từ khóa qua Query String (GET /api/categories/search?keyword=...)
        [HttpGet("search")]
        public IActionResult Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new { message = "Vui lòng nhập từ khóa!" });
            }
            // Lọc danh sách theo tên chứa từ khóa (không phân biệt chữ hoa/thường)
            var result = _categories
                .Where(c => c.CategoryName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return Ok(result);
        }

        // 4. CREATE: Thêm mới nhóm hàng (POST /api/categories)
        [HttpPost]
        public IActionResult Create([FromBody] Category newCat)
        {
            if (string.IsNullOrWhiteSpace(newCat.CategoryName))
            {
                return BadRequest(new { message = "Tên không được trống!" });
            }
            // Tự động tăng ID tiếp theo
            newCat.CategoryId = _categories.Count > 0 ? _categories.Max(c => c.CategoryId) + 1 : 1;
            _categories.Add(newCat);

            // Trả về mã 201 Created kèm đường dẫn dẫn tới bản ghi mới tạo
            return CreatedAtAction(nameof(GetById), new { id = newCat.CategoryId }, newCat);
        }

        // 5. UPDATE: Cập nhật thông tin nhóm hàng (PUT /api/categories/{id})
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Category updateCat)
        {
            var cat = _categories.FirstOrDefault(c => c.CategoryId == id);
=======
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly SupermarketDbContext _context;

        // Tiêm DbContext thông qua Constructor Injection
        public CategoriesController(SupermarketDbContext context)
        {
            _context = context;
        }

        // 1. READ: Lấy toàn bộ danh mục từ SQL Server
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.Categories.AsNoTracking().ToListAsync();
            return Ok(list);
        }

        // 2. READ: Lấy chi tiết 1 danh mục theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "Không tìm thấy nhóm hàng trong CSDL!" });
            }
            return Ok(category);
        }

        // 3. SEARCH: Tìm kiếm qua Query String trên SQL Server
        [HttpGet("search")]
        [Authorize(Roles = "Admin,Cashier,Warehouse")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new { message = "Vui lòng nhập từ khóa tìm kiếm!" });
            }
            // EF Core dịch biểu thức LINQ thành câu lệnh SQL LIKE tương ứng
            var result = await _context.Categories.AsNoTracking()
                .Where(c => c.CategoryName.Contains(keyword))
                .ToListAsync();
            return Ok(result);
        }

        // 4. CREATE: Thêm mới nhóm hàng vào Database
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Category newCat)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Categories.Add(newCat);
            await _context.SaveChangesAsync(); // Lưu thay đổi vào SQL Server

            return CreatedAtAction(nameof(GetById), new { id = newCat.CategoryId }, newCat);
        }

        // 5. UPDATE: Cập nhật nhóm hàng vào Database
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Category updateCat)
        {
            var cat = await _context.Categories.FindAsync(id);
>>>>>>> buoi-5
            if (cat == null)
            {
                return NotFound(new { message = "Không tìm thấy nhóm hàng cần sửa!" });
            }
<<<<<<< HEAD
            // Cập nhật giá trị mới
            cat.CategoryName = updateCat.CategoryName;
            cat.Description = updateCat.Description;

            // Trả về mã 204 NoContent biểu thị cập nhật thành công nhưng không cần trả về dữ liệu mới
            return NoContent();
        }

        // 6. DELETE: Xóa nhóm hàng theo ID (DELETE /api/categories/{id})
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var cat = _categories.FirstOrDefault(c => c.CategoryId == id);
=======

            cat.CategoryName = updateCat.CategoryName;
            cat.Description = updateCat.Description;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // 6. DELETE: Xóa nhóm hàng khỏi Database
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var cat = await _context.Categories.FindAsync(id);
>>>>>>> buoi-5
            if (cat == null)
            {
                return NotFound(new { message = "Không tìm thấy nhóm hàng cần xóa!" });
            }
<<<<<<< HEAD
            _categories.Remove(cat);
            return NoContent();
        }
    }
}
=======

            // Kiểm tra xem danh mục có sản phẩm nào không
            int productCount = await _context.Products.CountAsync(p => p.CategoryId == id);
            if (productCount > 0)
            {
                return BadRequest(new { message = $"Không thể xóa danh mục \"{cat.CategoryName}\" vì vẫn còn {productCount} sản phẩm trong danh mục này! Vui lòng chuyển hoặc xóa các sản phẩm thuộc danh mục trước." });
            }

            try
            {
                _context.Categories.Remove(cat);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { message = $"Không thể xóa danh mục \"{cat.CategoryName}\" vì vẫn còn dữ liệu sản phẩm liên kết ràng buộc!" });
            }
        }

        // 7. Kiểm tra quyền Admin (Chỉ tài khoản có Role = Admin mới được gọi)
        [HttpGet("admin-dashboard")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            return Ok(new { message = "Chào mừng Admin! Bạn có toàn quyền quản trị hệ thống siêu thị mini." });
        }

        // 8. Kiểm tra quyền chung cho nhân viên (Cả Admin và Cashier đều gọi được)
        [HttpGet("staff-pos")]
        [Authorize(Roles = "Admin,Cashier")]
        public async Task<IActionResult> GetStaffPos()
        {
            return Ok(new { message = "Màn hình POS Thu ngân sẵn sàng phục vụ bán hàng." });
        }

    }
}

>>>>>>> buoi-5
