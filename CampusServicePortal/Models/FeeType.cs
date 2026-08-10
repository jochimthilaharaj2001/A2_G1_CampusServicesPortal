using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.Models;

public class FeeType
{
    [Key]
    public int FeeTypeId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<FeePayment> FeePayments { get; set; } = new List<FeePayment>();
}
