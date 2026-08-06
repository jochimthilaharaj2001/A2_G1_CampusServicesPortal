using CampusServicePortal.Data;
using CampusServicePortal.Models;
using CampusServicePortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusServicePortal.Repositories.Implementation
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Student?> GetByIdAsync(int studentId)
        {
            return await _context.Students
                .Include(s => s.User)
                    .ThenInclude(u => u!.Role)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }

        public async Task<Student?> GetByUserIdAsync(int userId)
        {
            return await _context.Students
                .Include(s => s.User)
                    .ThenInclude(u => u!.Role)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<Student?> GetByIndexNumberAsync(string indexNumber)
        {
            return await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.IndexNumber == indexNumber);
        }

        public async Task<(IEnumerable<Student> Items, int TotalCount)> GetAllAsync(
            string? search, string? faculty, int page, int pageSize)
        {
            var query = _context.Students
                .Include(s => s.User)
                    .ThenInclude(u => u!.Role)
                .AsQueryable();

            // Filter by name, index number, or email
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                query = query.Where(s =>
                    s.User!.FullName.ToLower().Contains(lower) ||
                    s.IndexNumber.ToLower().Contains(lower) ||
                    s.User!.Email.ToLower().Contains(lower));
            }

            // Filter by faculty
            if (!string.IsNullOrWhiteSpace(faculty))
            {
                query = query.Where(s =>
                    s.Faculty.ToLower().Contains(faculty.ToLower()));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(s => s.User!.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task UpdateAsync(Student student)
        {
            _context.Students.Update(student);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
