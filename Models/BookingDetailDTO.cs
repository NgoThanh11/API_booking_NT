namespace Booking_thanhnt.Models
{
    public class BookingDetailDTO
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

        public List<BookingServiceDetailDto> Services { get; set; } = new();
    }
    public class BookingServiceDetailDto
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string? Service_text { get; set; }
        public decimal? Price { get; set; }
    }
}
