using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.Models;

public class CampusEvent
{
    [Key] public int EventId { get; set; }
    [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }
    [Required] public int VenueId { get; set; }
    [Required] public DateTime StartsAt { get; set; }
    [Required] public DateTime EndsAt { get; set; }
    [Range(1, 100000)] public int Capacity { get; set; }
    public bool UsesReservedSeating { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Venue? Venue { get; set; }
    public ICollection<EventSeat> EventSeats { get; set; } = new List<EventSeat>();
    public ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();
}
