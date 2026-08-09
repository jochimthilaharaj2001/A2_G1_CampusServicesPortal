using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.Models;

public class CertificateRequest
{
    [Key] public int CertificateRequestId { get; set; }
    [Required] public int UserId { get; set; }
    [Required] public int CertificateTypeId { get; set; }
    [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;
    [Required, MaxLength(40)] public string Status { get; set; } = "Pending";
    [MaxLength(1000)] public string? AdminNote { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public User? User { get; set; }
    public CertificateType? CertificateType { get; set; }
}
