using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.DTOs.MasterData
{
    public class FacultyDto
    {
        public int FacultyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public bool IsActive { get; set; }
        public int StudentCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateFacultyDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Code { get; set; }
    }

    public class UpdateFacultyDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Code { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class CertificateTypeDto
    {
        public int CertificateTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCertificateTypeDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Description { get; set; }
    }

    public class UpdateCertificateTypeDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
