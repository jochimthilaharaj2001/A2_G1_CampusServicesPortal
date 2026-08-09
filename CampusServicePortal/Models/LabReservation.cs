using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.Models
{
    public class LabReservation
    {
        [Key]
        public int LabReservationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int LabId { get; set; }

        public int? SeatId { get; set; }

        [Required]
        public DateTime ReservationDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [MaxLength(500)]
        public string? Purpose { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User? User { get; set; }

        public Lab? Lab { get; set; }

        public LabSeat? Seat { get; set; }
    }
}