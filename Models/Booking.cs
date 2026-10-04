namespace Booking_thanhnt.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int BranchId { get; set; }

        public int BarberId { get; set; }


        public string CustomerName { get; set; }

        public string Phone { get; set; }

        public DateTime BookingDate { get; set; }

        public string BookingTime { get; set; }

        public string? Email { get; set; }
        public string? Voucher { get; set; }
        public string? Barber_text { get; set; }
        public string? Branch_text { get; set; }
        // 1 Booking có nhiều BookingService
        public ICollection<BookingService> BookingServices { get; set; }
            = new List<BookingService>();
        public string Status { get; set; } = "PENDING";
    }
}
