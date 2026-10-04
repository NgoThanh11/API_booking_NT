namespace Booking_thanhnt.Models.DTOs
{
    public class BookingAdminDto
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

        public string Status { get; set; }

        public List<BookingServiceDto> Services { get; set; }
            = new List<BookingServiceDto>();
    }

    public class BookingServiceDto
    {
        public int Id { get; set; }

        public int ServiceId { get; set; }

        public string? Service_text { get; set; }

        public decimal? Price { get; set; }
    }
}