using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.DTOs.Students
{
    /// <summary>
    /// Fields a student may update on their own profile.
    /// IndexNumber is intentionally excluded — it is immutable after registration (BRD Business Rule).
    /// </summary>
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Degree Program is required")]
        [MaxLength(150)]
        public string DegreeProgram { get; set; } = string.Empty;
    }
}
