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
        [HttpGet]
        public async Task<IActionResult> GetBarbers()
        {
            var barbers = await _context.Barbers
                .ToListAsync();

            return Ok(barbers);
        }

        // GET: api/Barbers/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBarber(int id)
        {
            var barber = await _context.Barbers
                .FindAsync(id);

            if (barber == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy thợ cắt tóc"
                });
            }

            return Ok(barber);
        }

        // POST: api/Barbers
        [HttpPost]
        public async Task<IActionResult> CreateBarber([FromBody] Barber barber)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //_context.Barbers.Add(Barber);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetBarber),
                new { id = barber.BarberId },
                barber
            );
        }

        // PUT: api/Barbers/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBarber(
            int id,
            [FromBody] Barber barber)
        {
            if (id != barber.BarberId)
            {
                return BadRequest(new
                {
                    message = "BarberId không khớp"
                });
            }

            var existingBarber = await _context.Barbers
                .FindAsync(id);

            if (existingBarber == null)
            {
                return NotFound(new
                {
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBarber(int id)
        {
            var barber = await _context.Barbers
                .FindAsync(id);

            if (barber == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy thợ cắt tóc"
                });
            }

            _context.Barbers.Remove(barber);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Xóa thợ cắt tóc thành công"
            });
        }
    }
}