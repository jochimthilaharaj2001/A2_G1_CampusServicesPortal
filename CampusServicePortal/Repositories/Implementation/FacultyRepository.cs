using CampusServicePortal.Data;
using CampusServicePortal.Models;
using CampusServicePortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusServicePortal.Repositories.Implementation
{
    public class FacultyRepository : IFacultyRepository
    {
        private readonly ApplicationDbContext _context;

        public FacultyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Faculty>> GetAllAsync(bool activeOnly = false)
        {
            var query = _context.Faculties.AsQueryable();
            if (activeOnly)
                query = query.Where(f => f.IsActive);
            return await query.OrderBy(f => f.Name).ToListAsync();
        }

        public async Task<Faculty?> GetByIdAsync(int id)
            => await _context.Faculties.FirstOrDefaultAsync(f => f.FacultyId == id);

        public async Task<Faculty?> GetByNameAsync(string name)
            => await _context.Faculties
                .FirstOrDefaultAsync(f => f.Name.ToLower() == name.ToLower());

        public async Task AddAsync(Faculty faculty)
            => await _context.Faculties.AddAsync(faculty);

        public async Task UpdateAsync(Faculty faculty)
        {
            _context.Faculties.Update(faculty);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();

        public async Task<int> CountStudentsAsync(int facultyId)
            => await _context.Students.CountAsync(s => s.FacultyId == facultyId);
    }
}
