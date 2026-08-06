using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.Models
{
    /// <summary>
    /// Pre-loaded university registrar list used to validate student registration.
    /// Only students whose IndexNumber exists here can create a portal account.
    /// </summary>
    public class StudentMasterList
    {
        [Key]
        public int MasterListId { get; set; }

        [Required]
        [MaxLength(50)]
        public string IndexNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Faculty { get; set; } = string.Empty;

        [MaxLength(150)]
        public string DegreeProgram { get; set; } = string.Empty;

        public int EnrollmentYear { get; set; }

        /// <summary>
        /// Set to true once this index number has been used to create a Students account.
        /// Prevents duplicate registration.
        /// </summary>
        public bool IsRegistered { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
