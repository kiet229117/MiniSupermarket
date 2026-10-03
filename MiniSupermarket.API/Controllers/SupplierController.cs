using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSupermarket.API.Data;
using MiniSupermarket.API.Models;

namespace MiniSupermarket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly SupermarketDbContext _db;
        public SuppliersController(SupermarketDbContext db) => _db = db;

        // GET api/suppliers?keyword=dalat
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Supplier>>> GetAll([FromQuery] string? keyword)
        {
            var q = _db.Suppliers.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(s => s.SupplierName.Contains(keyword)
                              || (s.PhoneNumber != null && s.PhoneNumber.Contains(keyword)));
            return await q.OrderBy(s => s.SupplierName).ToListAsync();
        }

        [HttpGet("{id:int}")]

        public async Task<ActionResult<Supplier>> GetById(int id)
        {
            var item = await _db.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.SupplierId == id);
            return item == null ? NotFound() : item;
        }

        [HttpPost]
        public async Task<ActionResult<Supplier>> Create(Supplier model)
        {
            model.SupplierId = 0;
            _db.Suppliers.Add(model);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = model.SupplierId }, model);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, Supplier model)
        {
            if (id != model.SupplierId) return BadRequest("Id không khớp");

            var item = await _db.Suppliers.FindAsync(id);
            if (item == null) return NotFound();

            item.SupplierName = model.SupplierName;
            item.PhoneNumber = model.PhoneNumber;
            item.Email = model.Email;
            item.Address = model.Address;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Xoá NCC: sản phẩm vẫn còn, SupplierId tự về null (DeleteBehavior.SetNull)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Suppliers.FindAsync(id);
            if (item == null) return NotFound();

            _db.Suppliers.Remove(item);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}