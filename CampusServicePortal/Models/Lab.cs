using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.Models
{
    public enum LabType
    {
        Computer,
        Science
    }

    public class Lab
    {
        [Key]
        public int LabId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? RoomNumber { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public LabType LabType { get; set; } = LabType.Computer;

        [Required]
        public int Capacity { get; set; }

        // Navigation property
        public ICollection<LabSeat> LabSeats { get; set; } = new List<LabSeat>();
    }
}
