namespace Booking_thanhnt.Models
{
    public class Service
    {
        public int Id { get; set ; }
        public string Name { get; set; } = string.Empty;
        public string? Url { get; set; } 
        public string Description { get; set; } 
        public decimal Price { get; set; } 
        public int? DurationMinutes { get; set; }
        public bool Status { get; set; } = true; public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}
