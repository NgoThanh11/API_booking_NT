using Booking_thanhnt.Data;
using Booking_thanhnt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Booking_thanhnt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BarbersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BarbersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Barbers
        [HttpGet("get-all-baber")]
        public async Task<IActionResult> GetBarbers(int page = 1, int pageSize = 10)
        {
            // Không cho page < 1
            if (page < 1)
            { page = 1; }
            // Giới hạn pageSize để tránh request quá lớn
            if (pageSize < 1) { pageSize = 10; }
            if (pageSize > 100) { pageSize = 100; }
            // Query
            var query = _context.Barbers.AsNoTracking().OrderByDescending(x => x.BarberId);
            // Tổng số bản ghi
            var totalItems = await query.CountAsync();
            // Tổng số trang
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            // Nếu page vượt quá tổng số trang
            if (totalPages > 0 && page > totalPages) { page = totalPages; }
            // Lấy dữ liệu của trang hiện tại
            var barbers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.BarberId,
                    x.Name,
                    x.Experience,
                    x.Phone,
                    Branch = x.Branch == null ? null : new
                    {
                        x.Branch.BranchId,
                        x.Branch.BranchName
                    }
                })
                .ToListAsync();
            return Ok(new
            {
                data = barbers,
                page = page,
                pageSize = pageSize,
                totalItems = totalItems,
                totalPages = totalPages
            });
        }

        // GET: api/Barbers/1
        [HttpGet("get-by-id-baber")]
        public async Task<IActionResult> GetBarber(int id)
        {
            var barber = await _context.Barbers
                .AsNoTracking()
                .Where(b => b.BarberId == id)
                .Select(b => new
                {
                    b.BarberId,
                    b.Name,
                    b.Experience,
                    b.Phone,
                    b.BranchId,
                    b.CreateDate,
                    Branch = b.Branch == null ? null : new
                    {
                        b.Branch.BranchId,
                        b.Branch.BranchName,
                        b.Branch.BranchAddress,
                        b.Branch.BranchPhone
                    }
                })
                .FirstOrDefaultAsync();

            if (barber == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy thợ cắt tóc"
                });
            }

            return Ok(new
            {
                success = true,
                data = barber
            });
        }

        // POST: api/Barbers
        [HttpPost("create-baber")]
        public async Task<IActionResult> CreateBarber([FromBody] Barber barber)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            barber.CreateDate = DateTime.Now;
            if (barber.BranchId == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Thiếu thông tin chi nhánh",
                });
            }
            _context.Barbers.Add(barber);
            await _context.SaveChangesAsync();

            return Ok(
                new
                {
                    success = true,
                    message = "Thêm mới barber thành công",
                    data = barber
                }
            );
        }

        // PUT: api/Barbers/1
        [HttpPut("update-baber")]
        public async Task<IActionResult> UpdateBarber(
            int id,
            [FromBody] Barber barber)
        {
            if (id != barber.BarberId)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "BarberId không khớp"
                });
            }

            var existingBarber = await _context.Barbers
                .FindAsync(id);

            if (existingBarber == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy thợ cắt tóc"
                });
            }

            existingBarber.Name = barber.Name;
            existingBarber.Experience = barber.Experience;
            existingBarber.Phone = barber.Phone;

            await _context.SaveChangesAsync();

            return Ok(existingBarber);
        }

        // DELETE: api/Barbers/1
        [HttpDelete("delete-baber")]
        public async Task<IActionResult> DeleteBarber(int id)
        {
            var barber = await _context.Barbers
                .FindAsync(id);

            if (barber == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy thợ cắt tóc"
                });
            }

            _context.Barbers.Remove(barber);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Xóa thợ cắt tóc thành công"
            });
        }
    }
}