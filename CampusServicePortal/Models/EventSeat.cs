using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.Models;

public class EventSeat
{
    [Key] public int EventSeatId { get; set; }
    [Required] public int EventId { get; set; }
    [Required, MaxLength(50)] public string SeatNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public CampusEvent? Event { get; set; }
    public ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
}
