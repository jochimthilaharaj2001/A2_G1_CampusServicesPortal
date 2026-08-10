using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.DTOs.LabReservation
{
    public class UpdateLabReservationDTO
    {
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
    }
}