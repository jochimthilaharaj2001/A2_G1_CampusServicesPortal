using System.ComponentModel.DataAnnotations;

namespace CampusServicesPortal.Hostel.DTOs
{
    public class CreateHostelApplicationDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int HostelId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Semester { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? SpecialRequirements { get; set; }
    }
}