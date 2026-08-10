using System.ComponentModel.DataAnnotations;

namespace CampusServicesPortal.Hostel.DTOs
{
    public class CreateNotificationDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Message { get; set; } = string.Empty;
    }
}