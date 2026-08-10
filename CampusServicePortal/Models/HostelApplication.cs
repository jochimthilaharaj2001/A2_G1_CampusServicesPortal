using CampusServicePortal.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusServicesPortal.Hostel.Models
{
    public class HostelApplication
    {
        [Key]
        public int ApplicationId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        [Required]
        public int HostelId { get; set; }

        [ForeignKey("HostelId")]
        public Hostel? Hostel { get; set; }

        public int? RoomId { get; set; }

        [ForeignKey("RoomId")]
        public Room? Room { get; set; }

        [Required]
        [MaxLength(20)]
        public string Semester { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? SpecialRequirements { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime AppliedDate { get; set; } = DateTime.Now;
    }
}