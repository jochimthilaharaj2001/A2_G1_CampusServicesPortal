using System.ComponentModel.DataAnnotations;

namespace CampusServicesPortal.Hostel.DTOs
{
    public class UpdateHostelApplicationStatusDto
    {
        [Required]
        [RegularExpression("^(Approved|Rejected)$")]
        public string Status { get; set; } = string.Empty;
    }
}