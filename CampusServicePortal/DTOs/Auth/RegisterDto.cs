using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.DTOs.Auth
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Must match a record in StudentMasterList. Immutable after registration.
        /// </summary>
        [Required(ErrorMessage = "Index Number is required")]
        [MaxLength(50)]
        public string IndexNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Degree Program is required")]
        [MaxLength(150)]
        public string DegreeProgram { get; set; } = string.Empty;

        [Range(2000, 2100, ErrorMessage = "Enrollment year must be between 2000 and 2100")]
        public int EnrollmentYear { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }
    }
}