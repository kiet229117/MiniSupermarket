using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSupermarket.API.Data;
using MiniSupermarket.API.Models;

namespace MiniSupermarket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly SupermarketDbContext _db;
        public CustomersController(SupermarketDbContext db) => _db = db;

        // GET api/customers?keyword=0901
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetAll([FromQuery] string? keyword)
        {
            var q = _db.Customers.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(c => c.CustomerName.Contains(keyword) || c.PhoneNumber.Contains(keyword));
            return await q.OrderBy(c => c.CustomerName).ToListAsync();
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Cashier")]
        public async Task<ActionResult<Customer>> GetById(int id)
        {
            var item = await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.CustomerId == id);
            return item == null ? NotFound() : item;
        }

        // GET api/customers/phone/0901122334  (thu ngân tra khách theo SĐT)
        [HttpGet("phone/{phone}")]
        [Authorize(Roles = "Admin, Cashier")]
        public async Task<ActionResult<Customer>> GetByPhone(string phone)
        {
            var item = await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.PhoneNumber == phone);
            return item == null ? NotFound("Không tìm thấy khách hàng") : item;
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Cashier")]
        public async Task<ActionResult<Customer>> Create(Customer model)
        {
            if (await _db.Customers.AnyAsync(c => c.PhoneNumber == model.PhoneNumber))
                return Conflict("Số điện thoại đã được đăng ký");

            model.CustomerId = 0;
            _db.Customers.Add(model);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = model.CustomerId }, model);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, Customer model)
        {
            if (id != model.CustomerId) return BadRequest("Id không khớp");

            var item = await _db.Customers.FindAsync(id);
            if (item == null) return NotFound();

            if (await _db.Customers.AnyAsync(c => c.PhoneNumber == model.PhoneNumber && c.CustomerId != id))
                return Conflict("Số điện thoại đã được đăng ký");

            item.CustomerName = model.CustomerName;
            item.PhoneNumber = model.PhoneNumber;
            item.Address = model.Address;
            item.RewardPoints = model.RewardPoints;
            item.MembershipRank = model.MembershipRank;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Customers.FindAsync(id);
            if (item == null) return NotFound();

            if (await _db.Orders.AnyAsync(o => o.CustomerId == id))
                return Conflict("Khách hàng đã có hoá đơn, không thể xoá");

            _db.Customers.Remove(item);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}