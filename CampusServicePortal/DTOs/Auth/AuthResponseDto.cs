namespace CampusServicePortal.DTOs.Auth
{
    public class AuthResponseDto
    {
        public int UserId { get; set; }

        /// <summary>Student record id when the user is a student; null for Admin.</summary>
        public int? StudentId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string? Token { get; set; }

        public DateTime? Expiration { get; set; }

        public string? Message { get; set; }
    }
}