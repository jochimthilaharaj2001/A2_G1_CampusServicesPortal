using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.DTOs.Students
{
    public class AdminUpdateStudentDto
    {
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email format")]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Faculty is required")]
        [MaxLength(100)]
        public string Faculty { get; set; } = string.Empty;

        [Required(ErrorMessage = "Degree Program is required")]
        [MaxLength(150)]
        public string DegreeProgram { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enrollment Year is required")]
        [Range(2000, 2100)]
        public int EnrollmentYear { get; set; }

        [Phone]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        public bool IsActive { get; set; }

        public bool EmailVerified { get; set; }
    }
}
