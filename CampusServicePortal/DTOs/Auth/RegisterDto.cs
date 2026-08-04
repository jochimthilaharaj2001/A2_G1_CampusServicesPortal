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
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Student Number is required")]
        public string StudentNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Faculty is required")]
        public string Faculty { get; set; } = string.Empty;

        [Required(ErrorMessage = "Degree Program is required")]
        public string DegreeProgram { get; set; } = string.Empty;

        [Range(2000, 2100)]
        public int EnrollmentYear { get; set; }
    }
}