using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSupermarket.API.Data;
using MiniSupermarket.API.Models;

namespace MiniSupermarket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly SupermarketDbContext _db;

        public UsersController(SupermarketDbContext db)
        {
            _db = db;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? keyword)
        {
            var q = _db.Users.Include(u => u.Role).AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                q = q.Where(u => u.Username.Contains(keyword) || u.FullName.Contains(keyword) || (u.Email != null && u.Email.Contains(keyword)));
            }

            var list = await q.OrderBy(u => u.UserId)
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.FullName,
                    u.Email,
                    u.Phone,
                    u.IsActive,
                    u.RoleId,
                    Role = u.Role != null ? u.Role.RoleName : "CASHIER",
                    u.CreatedAt
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/Users/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var u = await _db.Users.Include(x => x.Role).AsNoTracking().FirstOrDefaultAsync(x => x.UserId == id);
            if (u == null) return NotFound("Không tìm thấy người dùng");

            return Ok(new
            {
                u.UserId,
                u.Username,
                u.FullName,
                u.Email,
                u.Phone,
                u.IsActive,
                u.RoleId,
                Role = u.Role != null ? u.Role.RoleName : "CASHIER",
                u.CreatedAt
            });
        }

        // POST: api/Users
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Tên đăng nhập và mật khẩu không được để trống");

            if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
                return Conflict("Tên đăng nhập đã tồn tại");

            // Tìm RoleId theo tên role ("ADMIN", "WAREHOUSE", "CASHIER")
            var targetRoleName = (dto.Role ?? "CASHIER").ToUpper();
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == targetRoleName)
                       ?? await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == "CASHIER");

            var user = new User
            {
                Username = dto.Username.Trim(),
                PasswordHash = HashPassword(dto.Password),
                FullName = string.IsNullOrWhiteSpace(dto.FullName) ? dto.Username : dto.FullName.Trim(),
                Email = dto.Email?.Trim(),
                Phone = dto.Phone?.Trim(),
                IsActive = dto.IsActive,
                RoleId = role != null ? role.RoleId : 3,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = user.UserId }, new
            {
                user.UserId,
                user.Username,
                user.FullName,
                user.Email,
                user.Phone,
                user.IsActive,
                user.RoleId,
                Role = role != null ? role.RoleName : "CASHIER",
                user.CreatedAt
            });
        }

        // PUT: api/Users/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound("Không tìm thấy người dùng");

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName.Trim();

            user.Email = dto.Email?.Trim();
            user.Phone = dto.Phone?.Trim();
            user.IsActive = dto.IsActive;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = HashPassword(dto.Password);
            }

            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.Role.ToUpper());
                if (role != null) user.RoleId = role.RoleId;
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        private static string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound("Không tìm thấy người dùng");

            // Không cho xóa chính tài khoản admin gốc
            if (user.Username.Equals("admin", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Không thể xóa tài khoản quản trị tối cao admin");

            // Kiểm tra xem đã từng xuất hóa đơn chưa
            if (await _db.Orders.AnyAsync(o => o.CashierId == id))
            {
                user.IsActive = false;
                await _db.SaveChangesAsync();
                return Ok(new { message = "Người dùng đã có hóa đơn trong hệ thống, chỉ vô hiệu hóa tài khoản" });
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }

    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Role { get; set; } = "CASHIER";
        public bool IsActive { get; set; } = true;
    }

    public class UpdateUserDto
    {
        public string? FullName { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Role { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
