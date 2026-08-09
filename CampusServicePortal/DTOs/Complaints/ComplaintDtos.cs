using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.DTOs.Complaints;

public class ComplaintCategoryDto
{
    public int ComplaintCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class SaveComplaintCategoryDto
{
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(300)] public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateComplaintDto
{
    [Required] public int ComplaintCategoryId { get; set; }
    [Required, MaxLength(2000)] public string Description { get; set; } = string.Empty;
}

public class UpdateComplaintStatusDto
{
    [Required, RegularExpression("^(Pending|In Progress|Resolved)$")]
    public string Status { get; set; } = string.Empty;
    [MaxLength(2000)] public string? ResolutionNote { get; set; }
}

public class ComplaintDto
{
    public int ComplaintId { get; set; }
    public int UserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string IndexNumber { get; set; } = string.Empty;
    public int ComplaintCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ResolutionNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
