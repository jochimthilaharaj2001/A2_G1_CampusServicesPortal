using CampusServicePortal.Data;
using CampusServicePortal.Models;
using CampusServicePortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusServicePortal.Repositories.Implementation
{
    public class CertificateTypeRepository : ICertificateTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public CertificateTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CertificateType>> GetAllAsync(bool activeOnly = false)
        {
            var query = _context.CertificateTypes.AsQueryable();
            if (activeOnly)
                query = query.Where(c => c.IsActive);
            return await query.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<CertificateType?> GetByIdAsync(int id)
            => await _context.CertificateTypes.FirstOrDefaultAsync(c => c.CertificateTypeId == id);

        public async Task<CertificateType?> GetByNameAsync(string name)
            => await _context.CertificateTypes
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());

        public async Task AddAsync(CertificateType entity)
            => await _context.CertificateTypes.AddAsync(entity);

        public async Task UpdateAsync(CertificateType entity)
        {
            _context.CertificateTypes.Update(entity);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
