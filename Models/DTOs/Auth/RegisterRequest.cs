namespace Booking_thanhnt.Models.DTOs.Auth
{
    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
