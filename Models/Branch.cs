using System.ComponentModel.DataAnnotations;
namespace Booking_thanhnt.Models
{
    public class Branch
    {
        [Key]
        public int BranchId { get; set; }
  
        public string BranchName { get; set; }
  
        public string BranchAddress { get; set; }
        public string BranchPhone { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}