using System.ComponentModel.DataAnnotations;

namespace CampusServicesPortal.Hostel.DTOs
{
    public class UpdateRoomDto
    {
        [Required]
        public int HostelId { get; set; }

        [Required]
        [MaxLength(20)]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        [Range(1, 20, ErrorMessage = "Capacity must be between 1 and 20.")]
        public int Capacity { get; set; }

        public int CurrentOccupancy { get; set; }

        [MaxLength(30)]
        public string? RoomType { get; set; }

        public bool IsActive { get; set; }
    }
}