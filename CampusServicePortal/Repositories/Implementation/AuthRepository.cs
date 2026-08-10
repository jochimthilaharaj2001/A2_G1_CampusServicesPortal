using CampusServicePortal.Data;
using CampusServicePortal.Models;
using CampusServicePortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusServicePortal.Repositories.Implementation
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _context;

        public AuthRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Student)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Student)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Student)
                .Include(u => u.RefreshTokens)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task AddStudentAsync(Student student)
        {
            await _context.Students.AddAsync(student);
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
        }

        public async Task<User?> GetUserByVerificationTokenAsync(string token)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Student)
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u =>
                    u.EmailVerificationToken == token);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}