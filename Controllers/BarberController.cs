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

        #region api search
        public class BarberSearchRequest
        {
            public string? Name { get; set; }
            public string? Phone { get; set; }
            public int? BranchId { get; set; }
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 10;
        }
        [HttpPost("search-barber")]
        public async Task<IActionResult> SearchBarber(
            [FromBody] BarberSearchRequest request)
        {

            string? name = request.Name;
            string? phone = request.Phone;
            int? branchId = request.BranchId;

            int page = 1;
            int pageSize = request.PageSize;


            if (page < 1 || pageSize < 1)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Page và PageSize phải lớn hơn 0"
                });
            }

            pageSize = Math.Min(pageSize, 100);

            var query = _context.Barbers
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                query = query.Where(b => b.Name.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(phone))
            {
                phone = phone.Trim();
                query = query.Where(b =>
                    b.Phone != null && b.Phone.Contains(phone));
            }

            if (branchId.HasValue)
            {
                query = query.Where(b =>
                    b.BranchId == branchId.Value);
            }

            var totalItems = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(
                (double)totalItems / pageSize);

            var data = await query
                .OrderByDescending(b => b.BarberId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new
                {
                    b.BarberId,
                    b.Name,
                    b.Phone,
                    b.Experience,
                    b.BranchId,
                    BranchName = b.Branch != null
                        ? b.Branch.BranchName
                        : null
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Tìm kiếm Barber thành công",
                data,
                pagination = new
                {
                    currentPage = page,
                    pageSize,
                    totalItems,
                    totalPages
                }
            });
        }
        #endregion

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
                    b.Age,
                    b.PlaceResidence,
                    b.IdCard,
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
            existingBarber.IdCard = barber.IdCard;
            existingBarber.PlaceResidence = barber.PlaceResidence;
            existingBarber.Age = barber.Age;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Cập nhật barber thành công",
                data = existingBarber
            });
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
                message = "Xóa thợ barber thành công"
            });
        }
    }
}