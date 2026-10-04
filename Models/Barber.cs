namespace Booking_thanhnt.Models
{
    public class Barber
    {
        public int BarberId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Experience { get; set; }

        public string? Phone { get; set; }
    }
}
