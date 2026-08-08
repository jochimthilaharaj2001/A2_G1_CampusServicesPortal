using CampusServicePortal.DTOs.MasterData;

namespace CampusServicePortal.Services.Interfaces
{
    public interface IFacultyService
    {
        Task<IEnumerable<FacultyDto>> GetAllAsync(bool activeOnly = false);
        Task<FacultyDto?> GetByIdAsync(int id);
        Task<FacultyDto> CreateAsync(CreateFacultyDto dto);
        Task<FacultyDto> UpdateAsync(int id, UpdateFacultyDto dto);
        Task DeactivateAsync(int id);
    }

    public interface ICertificateTypeService
    {
        Task<IEnumerable<CertificateTypeDto>> GetAllAsync(bool activeOnly = false);
        Task<CertificateTypeDto?> GetByIdAsync(int id);
        Task<CertificateTypeDto> CreateAsync(CreateCertificateTypeDto dto);
        Task<CertificateTypeDto> UpdateAsync(int id, UpdateCertificateTypeDto dto);
        Task DeactivateAsync(int id);
    }
}
