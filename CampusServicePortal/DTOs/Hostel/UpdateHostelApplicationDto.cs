using System.ComponentModel.DataAnnotations;

namespace CampusServicesPortal.Hostel.DTOs
{
    public class UpdateHostelApplicationDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int HostelId { get; set; }

        public int? RoomId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Semester { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? SpecialRequirements { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";
    }
}