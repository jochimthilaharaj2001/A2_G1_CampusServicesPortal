using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.Models
{
    /// <summary>
    /// Stores time-limited, single-use tokens for password reset (BRD Module 1.5).
    /// </summary>
    public class PasswordResetToken
    {
        [Key]
        public int TokenId { get; set; }

        public int UserId { get; set; }

        [Required]
        [MaxLength(512)]
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public User? User { get; set; }
    }
}
