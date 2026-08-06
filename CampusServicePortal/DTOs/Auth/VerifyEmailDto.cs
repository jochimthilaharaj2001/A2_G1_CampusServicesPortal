using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.DTOs.Auth
{
    public class VerifyEmailDto
    {
        [Required(ErrorMessage = "Verification token is required")]
        public string Token { get; set; } = string.Empty;
    }
}
