using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.Models;

public class FeePayment
{
    [Key]
    public int FeePaymentId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int FeeTypeId { get; set; }

    [Required, MaxLength(100)]
    public string BillingPeriod { get; set; } = string.Empty;

    [Required]
    [Range(typeof(decimal), "0.01", "99999999")]
    public decimal Amount { get; set; }

    [Required, MaxLength(30)]
    public string Status { get; set; } = "Outstanding";

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(50)]
    public string? ReceiptNumber { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public User? User { get; set; }
    public FeeType? FeeType { get; set; }
}
