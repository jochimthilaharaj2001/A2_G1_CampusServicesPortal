using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Username { get; set; }

        [Required]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int RoleId { get; set; }

        // Email Verification (BRD Module 1.3)
        public bool EmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiresAt { get; set; }

        // Navigation properties
        public Role? Role { get; set; }

        public Student? Student { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; }
            = new List<RefreshToken>();

        public ICollection<PasswordResetToken> PasswordResetTokens { get; set; }
            = new List<PasswordResetToken>();

        public ICollection<LabReservation> LabReservations { get; set; }
            = new List<LabReservation>();

        public ICollection<Complaint> Complaints { get; set; }
            = new List<Complaint>();

        public ICollection<FeePayment> FeePayments { get; set; }
            = new List<FeePayment>();
    }
}
