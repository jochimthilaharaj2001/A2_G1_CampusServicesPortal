using CampusServicePortal.Data;
using CampusServicePortal.Models;
using CampusServicePortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusServicePortal.Repositories.Implementation
{
    public class StudentMasterListRepository : IStudentMasterListRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentMasterListRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StudentMasterList?> GetByIndexNumberAsync(string indexNumber)
        {
            return await _context.StudentMasterList
                .FirstOrDefaultAsync(sml => sml.IndexNumber == indexNumber);
        }

        public async Task<IEnumerable<StudentMasterList>> GetAllAsync(string? search)
        {
            var query = _context.StudentMasterList.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                query = query.Where(sml =>
                    sml.IndexNumber.ToLower().Contains(lower) ||
                    sml.FullName.ToLower().Contains(lower) ||
                    sml.Faculty.ToLower().Contains(lower));
            }

            return await query.OrderBy(sml => sml.IndexNumber).ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<StudentMasterList> records)
        {
            await _context.StudentMasterList.AddRangeAsync(records);
        }

        public async Task MarkAsRegisteredAsync(string indexNumber)
        {
            var record = await _context.StudentMasterList
                .FirstOrDefaultAsync(sml => sml.IndexNumber == indexNumber);

            if (record != null)
            {
                record.IsRegistered = true;
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
