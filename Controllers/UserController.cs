using Booking_thanhnt.Data;
using Booking_thanhnt.Models;
using Booking_thanhnt.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Booking_thanhnt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region danh sách người dùng 

        [HttpGet("get-all-user")]
        public async Task<IActionResult> GetAllUsers(int page, int pageSize)
        {
            try
            {
                // VALIDATE PHÂN TRANG
                if (page < 1)
                {
                    page = 1;
                }

                if (pageSize < 1)
                {
                    pageSize = 10;
                }

                if (pageSize > 100)
                {
                    pageSize = 100;
                }

                // QUERY
                // Booking mới nhất lên đầu
                var query = _context.Users
                    .AsNoTracking()
                    .OrderByDescending(x => x.Id);


                // TỔNG SỐ BOOKING
                var totalItems = await query.CountAsync();

                // TỔNG SỐ TRANG

                var totalPages = (int)Math.Ceiling(
                    totalItems / (double)pageSize
                );

                // Nếu page vượt quá số trang
                if (totalPages > 0 && page > totalPages)
                {
                    page = totalPages;
                }

                // LẤY BOOKING CỦA TRANG

                var bookings = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // MAP DTO
                var users = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new
                {
                   x.Id,
                   x.Username,
                   x.FullName,
                   x.Role,
                   x.IsActive,
                   x.CreatedAt
                })
               .ToListAsync();
                // RESPONSE
                return Ok(new
                {
                    success = true,
                    data = users,

                    page = page,
                    pageSize = pageSize,
                    totalItems = totalItems,
                    totalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi khi lấy danh sách đặt lịch",
                    error = ex.Message
                });
            }

        }
        #endregion
        #region chi tiết người dùng

        [HttpGet("get-id-user")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.Username,
                    x.FullName,
                    x.Role,
                    x.IsActive,
                    x.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy tài khoản"
                });
            }

            return Ok(new
            {
                success = true,
                data = user
            });
        }
        #endregion
        #region thêm mới người dùng 

        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser(User request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng nhập tên đăng nhập"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.PasswordHash))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng nhập mật khẩu"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.FullName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng nhập họ tên"
                    });
                }

                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(x =>
                        x.Username == request.Username.Trim());

                if (existingUser != null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tên đăng nhập đã tồn tại"
                    });
                }

                if (request.Role != "Admin" &&
                    request.Role != "Customer")
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Role không hợp lệ"
                    });
                }

                // PasswordHash frontend gửi vào
                // nhưng backend phải hash lại
                request.PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        request.PasswordHash
                    );

                request.Id = 0;

                request.Username =
                    request.Username.Trim();

                request.FullName =
                    request.FullName.Trim();

                request.CreatedAt = DateTime.Now;

                _context.Users.Add(request);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Thêm tài khoản thành công",
                    data = new
                    {
                        request.Id,
                        request.Username,
                        request.FullName,
                        request.Role,
                        request.IsActive,
                        request.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi thêm tài khoản",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region cập nhật người dùng

        [HttpPut("update-user")]
        public async Task<IActionResult> UpdateUser(
            int id,
            User request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy tài khoản"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Username))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng nhập tên đăng nhập"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.FullName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng nhập họ tên"
                    });
                }

                var usernameExists = await _context.Users
                    .AnyAsync(x =>
                        x.Username == request.Username.Trim() &&
                        x.Id != id);

                if (usernameExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tên đăng nhập đã tồn tại"
                    });
                }

                if (request.Role != "Admin" &&
                    request.Role != "Customer")
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Role không hợp lệ"
                    });
                }

                user.Username =
                    request.Username.Trim();

                user.FullName =
                    request.FullName.Trim();

                user.Role = request.Role;

                user.IsActive = request.IsActive;

                // Nếu gửi password mới thì hash
                if (!string.IsNullOrWhiteSpace(
                    request.PasswordHash))
                {
                    user.PasswordHash =
                        BCrypt.Net.BCrypt.HashPassword(
                            request.PasswordHash
                        );
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật tài khoản thành công",
                    data = new
                    {
                        user.Id,
                        user.Username,
                        user.FullName,
                        user.Role,
                        user.IsActive,
                        user.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi cập nhật tài khoản",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region xóa người dùng 

        [HttpDelete("delete-user")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy tài khoản"
                    });
                }

                _context.Users.Remove(user);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Xóa tài khoản thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Không thể xóa tài khoản",
                    error = ex.Message
                });
            }
        }
        #endregion
    }
}