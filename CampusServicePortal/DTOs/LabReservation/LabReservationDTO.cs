using System;

namespace CampusServicePortal.DTOs.LabReservation
{
    public class LabReservationDTO
    {
        public int LabReservationId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int LabId { get; set; }
        public string? LabName { get; set; }
        public int? SeatId { get; set; }
        public string? SeatNumber { get; set; }
        public DateTime ReservationDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Purpose { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
    }
}
