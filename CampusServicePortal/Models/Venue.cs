using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.Models;

public class Venue
{
    [Key] public int VenueId { get; set; }
    [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string VenueType { get; set; } = "Event Hall";
    [Range(1, 100000)] public int Capacity { get; set; }
    [MaxLength(150)] public string? Location { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<CampusEvent> Events { get; set; } = new List<CampusEvent>();
}
