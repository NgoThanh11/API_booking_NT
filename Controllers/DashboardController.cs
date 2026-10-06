using Booking_thanhnt.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Booking_thanhnt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                var fromDate = today.AddDays(-6);

                #region TỔNG SỐ

                var totalBookings = await _context.Bookings
                    .CountAsync();

                var todayBookingsCount = await _context.Bookings
                    .CountAsync(x =>
                        x.BookingDate >= today &&
                        x.BookingDate < tomorrow &&
                        x.Status != "CANCELLED"
                    );

                var totalCustomers = await _context.Users
                    .CountAsync(x => x.Role == "Customer");

                var totalServices = await _context.Services
                    .CountAsync();

                var totalBarbers = await _context.Barbers
                    .CountAsync();

                var totalBranches = await _context.Branches
                    .CountAsync();
                #endregion

                #region LẤY BOOKING

                var bookings = await _context.Bookings
                    .AsNoTracking()
                    .Include(x => x.BookingServices)
                    .OrderByDescending(x => x.BookingDate)
                    .ThenByDescending(x => x.BookingTime)
                    .ToListAsync();
                #endregion

                #region DOANH THU HÔM NA
                var todayBookings = bookings
                    .Where(x =>
                        x.BookingDate >= today &&
                        x.BookingDate < tomorrow &&
                        x.Status != "CANCELLED"
                    )
                    .ToList();

                var todayRevenue = todayBookings
                    .SelectMany(x => x.BookingServices)
                    .Sum(x => x.Price);
                #endregion


                #region DOANH THU 7 NGÀY

                var revenue7Days = Enumerable
                    .Range(0, 7)
                    .Select(i =>
                    {
                        var date = fromDate.AddDays(i);

                        var revenue = bookings
                            .Where(x =>
                                x.BookingDate.Date == date.Date &&
                                x.Status != "CANCELLED"
                            )
                            .SelectMany(x => x.BookingServices)
                            .Sum(x => x.Price);

                        return new
                        {
                            date = date.ToString("yyyy-MM-dd"),
                            revenue = revenue
                        };
                    })
                    .ToList();

                #endregion

                #region  BOOKING GẦN ĐÂY
                var recentBookings = bookings
                    .Take(5)
                    .Select(x => new
                    {
                        id = x.Id,
                        customer = x.CustomerName,
                        phone = x.Phone,
                        service = string.Join(
                            ", ",
                            x.BookingServices
                                .Select(s => s.Service_text)
                        ),
                        barber = x.Barber_text,
                        date = x.BookingDate
                            .ToString("dd/MM/yyyy")
                            + " "
                            + x.BookingTime,
                        status = x.Status
                    })
                    .ToList();
                #endregion

                #region LỊCH HÔM NAY

                var todaySchedule = todayBookings
                    .OrderBy(x => x.BookingTime)
                    .Select(x => new
                    {
                        time = x.BookingTime,
                        customer = x.CustomerName,
                        service = string.Join(
                            ", ",
                            x.BookingServices
                                .Select(s => s.Service_text)
                        ),
                        status = x.Status
                    })
                    .ToList();
                #endregion
                #region DOANH THU THEO DỊCH VỤ

                var revenueByService = bookings
                    .Where(x => x.Status != "CANCELLED")
                    .SelectMany(x => x.BookingServices)
                    .GroupBy(x => x.Service_text)
                    .Select(g => new
                    {
                        name = g.Key,
                        revenue = g.Sum(x => x.Price)
                    })
                    .OrderByDescending(x => x.revenue)
                    .ToList();

                var totalServiceRevenue = revenueByService
                    .Sum(x => x.revenue ?? 0);

                var serviceRevenue = revenueByService
                    .Take(5)
                    .Select(x => new
                    {
                        name = x.name,
                        revenue = x.revenue ?? 0,

                        percent = totalServiceRevenue > 0
                            ? Math.Round(
                                (x.revenue ?? 0) * 100 /
                                totalServiceRevenue,
                                0
                              )
                            : 0
                    })
                    .ToList();
                #endregion
                // RESPONSE

                return Ok(new
                {
                    success = true,

                    stats = new
                    {
                        totalBookings,
                        todayBookings = todayBookingsCount,
                        todayRevenue,
                        totalCustomers,
                        totalServices,
                        totalBarbers,
                        totalBranches
                    },

                    revenue7Days,

                    recentBookings,

                    todaySchedule,

                    serviceRevenue
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi khi lấy dữ liệu Dashboard",
                    error = ex.Message
                });
            }
        }
    }
}