using CampusServicePortal.Models;

namespace CampusServicePortal.Repositories.Interfaces
{
    public interface ICertificateTypeRepository
    {
        Task<IEnumerable<CertificateType>> GetAllAsync(bool activeOnly = false);
        Task<CertificateType?> GetByIdAsync(int id);
        Task<CertificateType?> GetByNameAsync(string name);
        Task AddAsync(CertificateType entity);
        Task UpdateAsync(CertificateType entity);
        Task SaveChangesAsync();
    }
}
