using Azure;
using Booking_thanhnt.Data;
using Booking_thanhnt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Booking_thanhnt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. GET: api/Services
        // LẤY DANH SÁCH DỊCH VỤ
        [HttpGet]
        public async Task<IActionResult> GetServices(
           int page = 1,
           int pageSize = 10)
        {
            // Không cho page < 1
            if (page < 1)
            {
                page = 1;
            }

            // Giới hạn pageSize
            if (pageSize < 1)
            {
                pageSize = 10;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            // Query - CHƯA ToListAsync()
            var query = _context.Services
                .AsNoTracking()
                .OrderByDescending(s => s.Id);

            // Tổng số bản ghi
            var totalItems = await query.CountAsync();

            // Tổng số trang
            var totalPages = (int)Math.Ceiling(
                totalItems / (double)pageSize
            );

            // Nếu page vượt quá tổng số trang
            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            // Lấy dữ liệu của trang hiện tại
            var services = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data = services,
                page = page,
                pageSize = pageSize,
                totalItems = totalItems,
                totalPages = totalPages
            });
        }
        #region
        [HttpGet("get-service-detail")]
        public async Task<IActionResult> GetServiceDetail(int id)
        {
            try
            {
                var service = await _context.Services
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (service == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy dịch vụ"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        id = service.Id,
                        name = service.Name,
                        description = service.Description,
                        price = service.Price,
                        durationMinutes = service.DurationMinutes,
                        url = service.Url,
                        status = service.Status,
                        createdAt = service.CreatedAt,
                        updatedAt = service.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi khi lấy chi tiết dịch vụ",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region Api lấy ảnh từ backend hiển thị ra gd
        [HttpGet("get-service-images")]
        public IActionResult GetServiceImages()
        {
            try
            {
                var folderPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "images"
                );

                if (!Directory.Exists(folderPath))
                {
                    return Ok(new
                    {
                        success = true,
                        data = new List<string>()
                    });
                }

                var allowedExtensions = new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };

                var files = Directory
                    .GetFiles(folderPath)
                    .Where(file =>
                        allowedExtensions.Contains(
                            Path.GetExtension(file).ToLower()
                        )
                    )
                    .Select(file =>
                        $"/images/{Path.GetFileName(file)}"
                    )
                    .ToList();

                return Ok(new
                {
                    success = true,
                    data = files
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Không thể lấy danh sách hình ảnh",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region Thêm dịch vụ
        [HttpPost("create-service")]
        public async Task<IActionResult> CreateService([FromBody] Service service)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(service.Name))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tên dịch vụ không được để trống"
                    });
                }

                if (service.Price < 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Giá dịch vụ không hợp lệ"
                    });
                }

                if (service.DurationMinutes <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Thời gian dịch vụ phải lớn hơn 0"
                    });
                }

                if (string.IsNullOrWhiteSpace(service.Url))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng chọn hình ảnh"
                    });
                }

                var imagePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    service.Url.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                );

                if (!System.IO.File.Exists(imagePath))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Hình ảnh không tồn tại trong hệ thống"
                    });
                }

                service.CreatedAt = DateTime.Now;
                service.UpdatedAt = null;

                _context.Services.Add(service);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Thêm dịch vụ thành công",
                    data = service
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi khi thêm dịch vụ",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region update dịch vụ
        [HttpPut("update-service")]
        public async Task<IActionResult> UpdateService(
             int id,
             [FromBody] Service request)
        {
            try
            {
                var service = await _context.Services
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (service == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy dịch vụ"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tên dịch vụ không được để trống"
                    });
                }

                if (request.Price < 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Giá dịch vụ không hợp lệ"
                    });
                }

                if (request.DurationMinutes <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Thời gian dịch vụ phải lớn hơn 0"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Url))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng chọn hình ảnh"
                    });
                }

                // Kiểm tra ảnh có tồn tại trong backend không
                var imagePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    request.Url
                        .TrimStart('/')
                        .Replace("/", Path.DirectorySeparatorChar.ToString())
                );

                if (!System.IO.File.Exists(imagePath))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Hình ảnh không tồn tại trong hệ thống"
                    });
                }

                // Cập nhật
                service.Name = request.Name.Trim();
                service.Description = request.Description?.Trim() ?? "";
                service.Price = request.Price;
                service.DurationMinutes = request.DurationMinutes;
                service.Url = request.Url;
                service.Status = request.Status;
                service.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật dịch vụ thành công",
                    data = service
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi khi cập nhật dịch vụ",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region Xóa dịch vụ
        [HttpDelete("delete-service")]
        public async Task<IActionResult> DeleteService(int id)
        {
            try
            {
                var service = await _context.Services
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (service == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy dịch vụ"
                    });
                }

                _context.Services.Remove(service);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Xóa dịch vụ thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi khi xóa dịch vụ",
                    error = ex.Message
                });
            }
        }
        #endregion

    }
}
