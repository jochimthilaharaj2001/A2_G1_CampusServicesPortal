using CampusServicePortal.Models;

namespace CampusServicePortal.Repositories.Interfaces
{
    public interface IPasswordResetTokenRepository
    {
        Task CreateTokenAsync(PasswordResetToken token);
        Task<PasswordResetToken?> GetValidTokenAsync(string token);
        Task InvalidatePreviousTokensAsync(int userId);
        Task MarkTokenUsedAsync(int tokenId);
        Task SaveChangesAsync();
    }
}
