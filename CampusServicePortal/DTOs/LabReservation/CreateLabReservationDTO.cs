using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.DTOs.LabReservation
{
    public class CreateLabReservationDTO
    {
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
    }
}