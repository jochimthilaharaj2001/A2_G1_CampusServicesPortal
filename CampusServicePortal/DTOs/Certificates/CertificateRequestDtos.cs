using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.DTOs.Certificates;

public class CreateCertificateRequestDto { [Required] public int CertificateTypeId { get; set; } [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty; }
public class UpdateCertificateRequestStatusDto { [Required, RegularExpression("^(Pending|Approved|Rejected|Ready for Collection)$")] public string Status { get; set; } = string.Empty; [MaxLength(1000)] public string? AdminNote { get; set; } }
public class CertificateRequestDto { public int CertificateRequestId { get; set; } public int UserId { get; set; } public string StudentName { get; set; } = string.Empty; public string IndexNumber { get; set; } = string.Empty; public int CertificateTypeId { get; set; } public string CertificateTypeName { get; set; } = string.Empty; public string Reason { get; set; } = string.Empty; public string Status { get; set; } = string.Empty; public string? AdminNote { get; set; } public DateTime RequestedAt { get; set; } public DateTime? UpdatedAt { get; set; } }
