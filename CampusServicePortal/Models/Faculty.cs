using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.Models
{
    /// <summary>BRD Module 9 — university faculties master data.</summary>
    public class Faculty
    {
        [Key]
        public int FacultyId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Code { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
