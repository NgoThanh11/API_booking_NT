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
        public async Task<IActionResult> GetBranches(int page = 1, int pageSize = 10)
        {
            // Không cho page < 1
            if (page < 1)
            { page = 1; }
            // Giới hạn pageSize để tránh request quá lớn
            if (pageSize < 1) { pageSize = 10; }
            if (pageSize > 100) { pageSize = 100; }
            // Query
            var query = _context.Branches.AsNoTracking().OrderByDescending(x => x.BranchId);
            // Tổng số bản ghi
            var totalItems = await query.CountAsync();
            // Tổng số trang
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            // Nếu page vượt quá tổng số trang
            if (totalPages > 0 && page > totalPages) { page = totalPages; }
            // Lấy dữ liệu của trang hiện tại
            var branches = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return Ok(new
            {
                data = branches,
                page = page,
                pageSize = pageSize,
                totalItems = totalItems,
                totalPages = totalPages
            });
        }

        // GET: api/Branches/1
        // Xem chi tiết 1 chi nhánh
        [HttpGet("detail-branch")]
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
        [HttpPost("create-branch")]
        public async Task<IActionResult> CreateBranch(Branch branch)
        {
            branch.CreatedAt = DateTime.Now;

            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Thêm chi nhánh thành công",
                data = branch
            });
        }

        // Cập nhật chi nhánh
        [HttpPut("update-branch")]
        public async Task<IActionResult> UpdateBranch(
            int id,
            Branch branch)
        {
            try
            {
                var existingBranch = await _context.Branches
                    .FirstOrDefaultAsync(x => x.BranchId == id);

                if (existingBranch == null)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy chi nhánh"
                    });
                }

                existingBranch.BranchName = branch.BranchName;
                existingBranch.BranchAddress = branch.BranchAddress;
                existingBranch.BranchPhone = branch.BranchPhone;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật chi nhánh thành công",
                    data = new
                    {
                        branchId = existingBranch.BranchId,
                        branchName = existingBranch.BranchName,
                        branchAddress = existingBranch.BranchAddress,
                        branchPhone = existingBranch.BranchPhone,
                        createdAt = existingBranch.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi khi cập nhật chi nhánh",
                    error = ex.Message
                });
            }
        }


        // DELETE: api/Branches/1
        // Xóa chi nhánh
        [HttpDelete("delete-branch")]
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
                success = true,
                message = "Xóa chi nhánh thành công"
            });
        }
    }
}