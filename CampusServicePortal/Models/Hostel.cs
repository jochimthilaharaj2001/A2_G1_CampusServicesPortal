using System.ComponentModel.DataAnnotations;

namespace CampusServicesPortal.Hostel.Models
{
    public class Hostel
    {
        [Key]
        public int HostelId { get; set; }

        [Required]
        [MaxLength(100)]
        public string HostelName { get; set; }

        [Required]
        [MaxLength(10)]
        public string Gender { get; set; }

        [Required]
        [MaxLength(100)]
        public string Location { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}