using System.ComponentModel.DataAnnotations;
using CampusServicePortal.Models;

namespace CampusServicePortal.DTOs.Labs
{
    public class LabDto
    {
        public int LabId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? RoomNumber { get; set; }
        public bool IsActive { get; set; }
        public LabType LabType { get; set; }
        public int Capacity { get; set; }
        public int ActiveSeatCount { get; set; }
    }

    public class CreateLabDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? RoomNumber { get; set; }

        [Required]
        public LabType LabType { get; set; } = LabType.Computer;

        [Required]
        [Range(1, 500)]
        public int Capacity { get; set; }
    }

    public class UpdateLabDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? RoomNumber { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        [Range(1, 500)]
        public int Capacity { get; set; }
    }

    public class LabSeatDto
    {
        public int SeatId { get; set; }
        public int LabId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CreateLabSeatDto
    {
        [Required]
        [MaxLength(50)]
        public string SeatNumber { get; set; } = string.Empty;
    }

    public class SeatAvailabilityDto
    {
        public int SeatId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public bool IsBooked { get; set; }
    }
}
