using System.ComponentModel.DataAnnotations;

namespace CampusServicePortal.Models
{
    public class RefreshToken
    {
        [Key]
        public int RefreshTokenId { get; set; }

        public string Token { get; set; }
            = string.Empty;

        public DateTime ExpiryDate { get; set; }

        public bool IsRevoked { get; set; }

        public int UserId { get; set; }

        public User? User { get; set; }
    }
}
