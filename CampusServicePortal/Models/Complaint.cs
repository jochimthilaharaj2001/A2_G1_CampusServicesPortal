using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.Models;

public class Complaint
{
    [Key]
    public int ComplaintId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int ComplaintCategoryId { get; set; }

    [Required, MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string Status { get; set; } = "Pending";

    [MaxLength(2000)]
    public string? ResolutionNote { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public User? User { get; set; }
    public ComplaintCategory? Category { get; set; }
}
