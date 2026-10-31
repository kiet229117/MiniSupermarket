using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSupermarket.API.Data;
using MiniSupermarket.API.Models;

namespace MiniSupermarket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrandsController : ControllerBase
    {
        private readonly SupermarketDbContext _db;
        public BrandsController(SupermarketDbContext db) => _db = db;
        [HttpGet]
   

        // GET api/brands?keyword=quaker&activeOnly=true
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Brand>>> GetAll([FromQuery] string? keyword, [FromQuery] bool activeOnly = false)
        {
            var q = _db.Brands.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(b => b.BrandName.Contains(keyword));
            if (activeOnly)
                q = q.Where(b => b.IsActive);
            return await q.OrderBy(b => b.BrandName).ToListAsync();
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin, Cashier")]
        public async Task<ActionResult<Brand>> GetById(int id)
        {
            var item = await _db.Brands.AsNoTracking().FirstOrDefaultAsync(b => b.BrandId == id);
            return item == null ? NotFound() : item;
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Cashier")]
        public async Task<ActionResult<Brand>> Create(Brand model)
        {
            if (await _db.Brands.AnyAsync(b => b.BrandName == model.BrandName))
                return Conflict("Tên thương hiệu đã tồn tại");

            model.BrandId = 0;
            _db.Brands.Add(model);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = model.BrandId }, model);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, Brand model)
        {
            if (id != model.BrandId) return BadRequest("Id không khớp");

            var item = await _db.Brands.FindAsync(id);
            if (item == null) return NotFound();

            if (await _db.Brands.AnyAsync(b => b.BrandName == model.BrandName && b.BrandId != id))
                return Conflict("Tên thương hiệu đã tồn tại");

            item.BrandName = model.BrandName;
            item.Country = model.Country;
            item.Description = model.Description;
            item.IsActive = model.IsActive;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Xoá thương hiệu: sản phẩm vẫn còn, BrandId tự về null (DeleteBehavior.SetNull)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Brands.FindAsync(id);
            if (item == null) return NotFound();

            _db.Brands.Remove(item);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}