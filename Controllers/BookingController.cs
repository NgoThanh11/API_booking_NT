using Booking_thanhnt.Data;
using Booking_thanhnt.Models;
using Booking_thanhnt.Models.DTOs;
using Booking_thanhnt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
namespace Booking_thanhnt.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<BookingController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        #region  Đặt lịch Booking
        [HttpPost("create-booking")]
        public async Task<IActionResult> CreateBooking(
            [FromBody] CreateBookingRequest request)
        {
            try
            {
                // 1. Kiểm tra dữ liệu đầu vào
                if (request == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu booking không hợp lệ"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.CustomerName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng nhập tên khách hàng"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Phone))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng nhập số điện thoại"
                    });
                }

                // 2. Kiểm tra Branch
                var branch = await _context.Branches
                    .FirstOrDefaultAsync(x => x.BranchId == request.BranchId);

                if (branch == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cơ sở không tồn tại"
                    });
                }

                // 3. Kiểm tra Barber
                //var barber = await _context.Barbers
                //    .FirstOrDefaultAsync(x => x.BarberId == request.BarberId);

                //if (barber == null)
                //{
                //    return BadRequest(new
                //    {
                //        success = false,
                //        message = "Barber không tồn tại"
                //    });
                //}

                // 4. Kiểm tra Service
                if (request.ServiceIds == null || !request.ServiceIds.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng chọn ít nhất một dịch vụ"
                    });
                }

                var services = await _context.Services
                    .Where(x => request.ServiceIds.Contains(x.Id))
                    .ToListAsync();

                if (services.Count != request.ServiceIds.Distinct().Count())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Có dịch vụ không tồn tại"
                    });
                }

                // 5. Kiểm tra trùng lịch
                var existedBooking = await _context.Bookings
                    .AnyAsync(x =>
                        x.BarberId == request.BarberId &&
                        x.BookingDate.Date == request.BookingDate.Date &&
                        x.BookingTime == request.BookingTime &&
                        x.Status != "CANCELLED"
                    );

                if (existedBooking)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Barber đã có lịch vào thời gian này"
                    });
                }

                // 6. Tạo Booking
                var booking = new Booking
                {
                    BranchId = request.BranchId,
                    BarberId = request.BarberId,

                    CustomerName = request.CustomerName.Trim(),
                    Phone = request.Phone.Trim(),

                    BookingDate = request.BookingDate,
                    BookingTime = request.BookingTime,

                    Email = request.Email?.Trim(),
                    Voucher = request.Voucher?.Trim(),
                    Branch_text = request.Branch_text,
                    Barber_text = request.Barber_text,
                    Status = "PENDING"


                };
                foreach (var service in services)
                {
                    booking.BookingServices.Add(new BookingService
                    {
                        ServiceId = service.Id,
                        Service_text = service.Name,
                        Price = service.Price
                    });
                }

                // 7. Thêm vào database
                _context.Bookings.Add(booking);

                await _context.SaveChangesAsync();

                // 8. Gửi email xác nhận sau khi lưu booking thành công
                if (!string.IsNullOrWhiteSpace(booking.Email))
                {
                    try
                    {
                        var serviceDetails = string.Join(
                            "<br/>",
                            booking.BookingServices.Select(x =>
                                $"{WebUtility.HtmlEncode(x.Service_text)} - " +
                                $"{x.Price:N0} VNĐ"
                            )
                        );

                        var totalPrice = booking.BookingServices
                            .Sum(x => x.Price);

                        var customerName =
                            WebUtility.HtmlEncode(booking.CustomerName);

                        var bookingTime =
                            WebUtility.HtmlEncode(
                                Convert.ToString(booking.BookingTime) ?? ""
                            );

                        var barberName =
                            WebUtility.HtmlEncode(booking.Barber_text ?? "");

                        var branchName =
                            WebUtility.HtmlEncode(booking.Branch_text ?? "");

                        var emailBody = $@"
                        <div style='font-family:Arial,sans-serif;
                                    max-width:600px;margin:auto;
                                    line-height:1.7;color:#333'>
                            <h2 style='color:#15397F'>
                                Đặt lịch thành công!
                            </h2>

                            <p>Xin chào <b>{customerName}</b>,</p>

                            <p>
                                Cảm ơn bạn đã đặt lịch tại Barber Smile11.
                                Dưới đây là thông tin lịch hẹn của bạn:
                            </p>

                            <div style='background:#f5f7fb;
                                        padding:16px;border-radius:8px'>
                                <p><b>Mã lịch đặt:</b> {booking.Id}</p>
                                <p><b>Khách hàng:</b> {customerName}</p>
                                <p><b>Số điện thoại:</b>
                                    {WebUtility.HtmlEncode(booking.Phone)}</p>
                                <p><b>Chi nhánh:</b> {branchName}</p>
                                <p><b>Thợ cắt tóc:</b> {barberName}</p>
                                <p><b>Ngày đặt:</b>
                                    {booking.BookingDate:dd/MM/yyyy}</p>
                                <p><b>Giờ đặt:</b> {bookingTime}</p>

                                <hr/>

                                <p><b>Dịch vụ:</b></p>
                                {serviceDetails}

                                <p><b>Tổng tiền dịch vụ:</b>
                                    {totalPrice:N0} VNĐ</p>

                                <p><b>Trạng thái:</b> Chờ xác nhận</p>
                            </div>

                            <p>
                                Vui lòng đến đúng giờ hẹn.
                                Chúng tôi sẽ liên hệ nếu cần thêm thông tin.
                            </p>

                            <p>Trân trọng,<br/>
                               <b>Barber Smile11</b></p>
                        </div>";

                        await _emailService.SendEmailAsync(
                            booking.Email,
                            "Xác nhận đặt lịch Barber #" + booking.Id,
                            emailBody
                        );
                    }
                    catch (Exception emailEx)
                    {
                        // Email lỗi không làm mất booking đã lưu.
                        _logger.LogError(
                            emailEx,
                            "Không gửi được email xác nhận cho booking {BookingId}",
                            booking.Id
                        );
                    }
                }

                // 9. Trả kết quả
                return Ok(new
                {
                    success = true,
                    message = "Đặt lịch thành công",

                    data = new
                    {
                        booking.Id,
                        booking.BranchId,
                        booking.BarberId,
                        booking.CustomerName,
                        booking.Phone,
                        booking.BookingDate,
                        booking.BookingTime,
                        booking.Email,
                        booking.Voucher,
                        booking.Status,
                        booking.Branch_text,
                        booking.Barber_text,
                        services = booking.BookingServices.Select(x => new
                        {
                            serviceId = x.ServiceId,
                            serviceText = x.Service_text,
                            price = x.Price
                        }).ToList()


                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi tạo booking",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region Lấy danh sách booking
        [HttpGet("get-all-booking")]
        public async Task<IActionResult> GetAllBookings(
            int page = 1,
            int pageSize = 10)
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
                var query = _context.Bookings
                    .AsNoTracking()
                    .Include(x => x.BookingServices)
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
                var result = bookings.Select(x => new BookingAdminDto
                {
                    Id = x.Id,
                    BranchId = x.BranchId,
                    BarberId = x.BarberId,

                    CustomerName = x.CustomerName,
                    Phone = x.Phone,

                    BookingDate = x.BookingDate,
                    BookingTime = x.BookingTime,

                    Email = x.Email,
                    Voucher = x.Voucher,

                    Barber_text = x.Barber_text,
                    Branch_text = x.Branch_text,

                    Status = x.Status,

                    Services = x.BookingServices
                        .Select(bs => new BookingServiceDto
                        {
                            Id = bs.Id,
                            ServiceId = bs.ServiceId,
                            Service_text = bs.Service_text,
                            Price = bs.Price
                        })
                        .ToList()
                }).ToList();
                // RESPONSE
                // RESPONSE
                return Ok(new
                {
                    success = true,
                    data = result,

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

        #region Chi tiết đặt lịch
        [HttpGet("get-booking-detail")]
        public async Task<IActionResult> GetBookingDetail(int id)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(x => x.BookingServices)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (booking == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy lịch đặt"
                    });
                }

                var result = new BookingDetailDTO
                {
                    Id = booking.Id,
                    BranchId = booking.BranchId,
                    BarberId = booking.BarberId,

                    CustomerName = booking.CustomerName,
                    Phone = booking.Phone,

                    BookingDate = booking.BookingDate,
                    BookingTime = booking.BookingTime,

                    Email = booking.Email,
                    Voucher = booking.Voucher,

                    Barber_text = booking.Barber_text,
                    Branch_text = booking.Branch_text,

                    Status = booking.Status,

                    Services = booking.BookingServices
                        .Select(x => new BookingServiceDetailDto
                        {
                            Id = x.Id,
                            ServiceId = x.ServiceId,
                            Service_text = x.Service_text,
                            Price = x.Price
                        })
                        .ToList()
                };

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi khi lấy chi tiết lịch đặt",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region cập nhật trạng thái
        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateBookingStatus(
            int id,
            [FromBody] string status)
        {
            try
            {
                var booking = await _context.Bookings
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (booking == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy lịch đặt"
                    });
                }

                var allowedStatuses = new[]
                {
                    "PENDING",
                    "CONFIRMED",
                    "COMPLETED",
                    "CANCELLED"
                };

                status = status?.ToUpper();

                if (!allowedStatuses.Contains(status))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Trạng thái không hợp lệ"
                    });
                }

                booking.Status = status;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật trạng thái thành công",
                    data = new
                    {
                        booking.Id,
                        booking.Status
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi khi cập nhật trạng thái",
                    error = ex.Message
                });
            }
        }

        #endregion
    }
}

