using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.Models
{
    public class LabSeat
    {
        [Key]
        public int SeatId { get; set; }

        [Required]
        public int LabId { get; set; }

        [Required]
        [MaxLength(50)]
        public string SeatNumber { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public Lab? Lab { get; set; }

        public ICollection<LabReservation> LabReservations { get; set; }
            = new List<LabReservation>();
    }
}