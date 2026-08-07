using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusServicesPortal.Hostel.Models
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }

        [Required]
        public int HostelId { get; set; }

        [ForeignKey("HostelId")]
        public Hostel? Hostel { get; set; }

        [Required]
        [MaxLength(20)]
        public string RoomNumber { get; set; }

        [Required]
        public int Capacity { get; set; }

        public int CurrentOccupancy { get; set; } = 0;

        [MaxLength(30)]
        public string? RoomType { get; set; }

        public bool IsActive { get; set; } = true;
    }
}