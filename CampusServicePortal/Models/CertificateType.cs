using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.Models
{
    /// <summary>BRD Module 9 — certificate types master data.</summary>
    public class CertificateType
    {
        [Key]
        public int CertificateTypeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
