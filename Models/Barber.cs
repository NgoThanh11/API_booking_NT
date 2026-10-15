namespace Booking_thanhnt.Models
{
    public class Barber
    {
        public int BarberId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Experience { get; set; }

        public string? Phone { get; set; }
        public int BranchId { get; set; }
        public string? PlaceResidence { get; set; }
        public string? IdCard { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? Age { get; set; }

        // Navigation property
        public Branch? Branch { get; set; }
    }
}
