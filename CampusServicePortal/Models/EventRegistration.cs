using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.Models;

public class EventRegistration
{
    [Key] public int EventRegistrationId { get; set; }
    [Required] public int EventId { get; set; }
    [Required] public int UserId { get; set; }
    public int? EventSeatId { get; set; }
    [Required, MaxLength(30)] public string Status { get; set; } = "Confirmed";
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }
    public CampusEvent? Event { get; set; }
    public User? User { get; set; }
    public EventSeat? Seat { get; set; }
}
