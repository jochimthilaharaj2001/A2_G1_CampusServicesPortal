using System.ComponentModel.DataAnnotations;

namespace CampusServicesPortal.Hostel.DTOs
{
    public class UpdateHostelDto
    {
        [Required]
        [MaxLength(100)]
        public string HostelName { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Location { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}