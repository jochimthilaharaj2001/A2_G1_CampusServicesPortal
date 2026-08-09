using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.DTOs.Auth
{
    public class LoginDto
    {
        public string? Email { get; set; }

        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        public string? Role { get; set; }
    }
}