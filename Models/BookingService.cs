namespace Booking_thanhnt.Models
{
    public class BookingService
    {
        public int Id { get; set; }

        public int BookingId { get; set; }

        public int ServiceId { get; set; }

        public string? Service_text { get; set; }

        public decimal? Price { get; set; }

        // Navigation
        public Booking Booking { get; set; }
    }
}
