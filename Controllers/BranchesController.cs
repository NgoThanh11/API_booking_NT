using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Booking_thanhnt.Data;
using Booking_thanhnt.Models;

namespace Booking_thanhnt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BranchesController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: api/Branches
        // Lấy danh sách tất cả chi nhánh
        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            var branches = await _context.Branches
                .ToListAsync();

            return Ok(branches);
        }


        // GET: api/Branches/1
        // Xem chi tiết 1 chi nhánh
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBranch(int id)
        {
            var branch = await _context.Branches
                .FirstOrDefaultAsync(x => x.BranchId == id);

            if (branch == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy chi nhánh"
                });
            }

            return Ok(branch);
        }


        // POST: api/Branches
        // Thêm chi nhánh
        [HttpPost]
        public async Task<IActionResult> CreateBranch(Branch branch)
        {
            branch.CreatedAt = DateTime.Now;

            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Thêm chi nhánh thành công",
                data = branch
            });
        }


        // PUT: api/Branches/1
        // Cập nhật chi nhánh
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBranch(int id, Branch branch)
        {
            if (id != branch.BranchId)
            {
                return BadRequest(new
                {
                    message = "Id không khớp"
                });
            }


            _context.Entry(branch).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật chi nhánh thành công"
            });
        }


        // DELETE: api/Branches/1
        // Xóa chi nhánh
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBranch(int id)
        {
            var branch = await _context.Branches
                .FindAsync(id);


            if (branch == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy chi nhánh"
                });
            }


            _context.Branches.Remove(branch);

            await _context.SaveChangesAsync();


            return Ok(new
            {
                message = "Xóa chi nhánh thành công"
            });
        }
    }
}