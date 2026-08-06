using CampusServicePortal.Data;
using CampusServicePortal.Models;
using CampusServicePortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusServicePortal.Repositories.Implementation
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public PasswordResetTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateTokenAsync(PasswordResetToken token)
        {
            await _context.PasswordResetTokens.AddAsync(token);
        }

        public async Task<PasswordResetToken?> GetValidTokenAsync(string token)
        {
            return await _context.PasswordResetTokens
                .Include(prt => prt.User)
                .FirstOrDefaultAsync(prt =>
                    prt.Token == token &&
                    !prt.IsUsed &&
                    prt.ExpiresAt > DateTime.UtcNow);
        }

        public async Task InvalidatePreviousTokensAsync(int userId)
        {
            var activeTokens = await _context.PasswordResetTokens
                .Where(prt => prt.UserId == userId && !prt.IsUsed)
                .ToListAsync();

            foreach (var t in activeTokens)
            {
                t.IsUsed = true;
            }
        }

        public async Task MarkTokenUsedAsync(int tokenId)
        {
            var token = await _context.PasswordResetTokens.FindAsync(tokenId);
            if (token != null)
            {
                token.IsUsed = true;
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
