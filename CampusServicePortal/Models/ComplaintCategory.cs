using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.Models;

public class ComplaintCategory
{
    [Key]
    public int ComplaintCategoryId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}
