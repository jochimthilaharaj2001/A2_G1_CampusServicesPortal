namespace CampusServicePortal.DTOs.Students
{
    /// <summary>
    /// Read-only student profile returned to the frontend.
    /// Never exposes internal fields like password hash or raw tokens.
    /// </summary>
    public class StudentProfileDto
    {
        public int StudentId { get; set; }
        public int UserId { get; set; }

        // From User
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public bool EmailVerified { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        // From Student
        public string IndexNumber { get; set; } = string.Empty;
        public string Faculty { get; set; } = string.Empty;
        public string DegreeProgram { get; set; } = string.Empty;
        public int EnrollmentYear { get; set; }
        public string? ContactNumber { get; set; }
        public string? Address { get; set; }
        public DateTime? DeactivatedAt { get; set; }

        // Role
        public string Role { get; set; } = "Student";

        /// <summary>
        /// Summary of activity across other modules (empty until Modules 2–8 exist).
        /// </summary>
        public StudentActivitySummaryDto ActivitySummary { get; set; } = new();
    }
}
