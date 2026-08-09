using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.Models
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string IndexNumber { get; set; } = string.Empty;

        /// <summary>FK to Faculties (Module 9). Nullable for legacy rows until backfilled.</summary>
        public int? FacultyId { get; set; }

        [MaxLength(100)]
        public string Faculty { get; set; } = string.Empty;

        [MaxLength(150)]
        public string DegreeProgram { get; set; } = string.Empty;

        public int EnrollmentYear { get; set; }

        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        // Activation / Deactivation
        public bool IsActive { get; set; } = true;
        public DateTime? DeactivatedAt { get; set; }

        // Navigation properties
        public User? User { get; set; }
        public Faculty? FacultyNav { get; set; }
    }
}
