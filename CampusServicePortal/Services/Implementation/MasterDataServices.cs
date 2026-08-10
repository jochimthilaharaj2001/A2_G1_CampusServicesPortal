using CampusServicePortal.DTOs.MasterData;
using CampusServicePortal.Models;
using CampusServicePortal.Repositories.Interfaces;
using CampusServicePortal.Services.Interfaces;

namespace CampusServicePortal.Services.Implementation
{
    public class FacultyService : IFacultyService
    {
        private readonly IFacultyRepository _repo;

        public FacultyService(IFacultyRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<FacultyDto>> GetAllAsync(bool activeOnly = false)
        {
            var items = await _repo.GetAllAsync(activeOnly);
            var result = new List<FacultyDto>();
            foreach (var f in items)
            {
                result.Add(await MapAsync(f));
            }
            return result;
        }

        public async Task<FacultyDto?> GetByIdAsync(int id)
        {
            var f = await _repo.GetByIdAsync(id);
            return f == null ? null : await MapAsync(f);
        }

        public async Task<FacultyDto> CreateAsync(CreateFacultyDto dto)
        {
            var existing = await _repo.GetByNameAsync(dto.Name.Trim());
            if (existing != null)
                throw new InvalidOperationException("A faculty with this name already exists.");

            var faculty = new Faculty
            {
                Name = dto.Name.Trim(),
                Code = string.IsNullOrWhiteSpace(dto.Code) ? null : dto.Code.Trim().ToUpperInvariant(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(faculty);
            await _repo.SaveChangesAsync();
            return await MapAsync(faculty);
        }

        public async Task<FacultyDto> UpdateAsync(int id, UpdateFacultyDto dto)
        {
            var faculty = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Faculty not found.");

            var nameClash = await _repo.GetByNameAsync(dto.Name.Trim());
            if (nameClash != null && nameClash.FacultyId != id)
                throw new InvalidOperationException("A faculty with this name already exists.");

            faculty.Name = dto.Name.Trim();
            faculty.Code = string.IsNullOrWhiteSpace(dto.Code) ? null : dto.Code.Trim().ToUpperInvariant();
            faculty.IsActive = dto.IsActive;
            faculty.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(faculty);
            await _repo.SaveChangesAsync();
            return await MapAsync(faculty);
        }

        public async Task DeactivateAsync(int id)
        {
            var faculty = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Faculty not found.");

            var studentCount = await _repo.CountStudentsAsync(id);
            if (studentCount > 0 && faculty.IsActive)
            {
                // BRD: cannot hard-delete while students linked — deactivate instead
                faculty.IsActive = false;
                faculty.UpdatedAt = DateTime.UtcNow;
                await _repo.UpdateAsync(faculty);
                await _repo.SaveChangesAsync();
                return;
            }

            faculty.IsActive = false;
            faculty.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(faculty);
            await _repo.SaveChangesAsync();
        }

        private async Task<FacultyDto> MapAsync(Faculty f)
        {
            return new FacultyDto
            {
                FacultyId = f.FacultyId,
                Name = f.Name,
                Code = f.Code,
                IsActive = f.IsActive,
                StudentCount = await _repo.CountStudentsAsync(f.FacultyId),
                CreatedAt = f.CreatedAt
            };
        }
    }

    public class CertificateTypeService : ICertificateTypeService
    {
        private readonly ICertificateTypeRepository _repo;

        public CertificateTypeService(ICertificateTypeRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<CertificateTypeDto>> GetAllAsync(bool activeOnly = false)
        {
            var items = await _repo.GetAllAsync(activeOnly);
            return items.Select(Map);
        }

        public async Task<CertificateTypeDto?> GetByIdAsync(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            return item == null ? null : Map(item);
        }

        public async Task<CertificateTypeDto> CreateAsync(CreateCertificateTypeDto dto)
        {
            var existing = await _repo.GetByNameAsync(dto.Name.Trim());
            if (existing != null)
                throw new InvalidOperationException("A certificate type with this name already exists.");

            var entity = new CertificateType
            {
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<CertificateTypeDto> UpdateAsync(int id, UpdateCertificateTypeDto dto)
        {
            var entity = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Certificate type not found.");

            var nameClash = await _repo.GetByNameAsync(dto.Name.Trim());
            if (nameClash != null && nameClash.CertificateTypeId != id)
                throw new InvalidOperationException("A certificate type with this name already exists.");

            entity.Name = dto.Name.Trim();
            entity.Description = dto.Description?.Trim();
            entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(entity);
            await _repo.SaveChangesAsync();
            return Map(entity);
        }

        public async Task DeactivateAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Certificate type not found.");

            // Soft-deactivate (hard delete blocked once certificate requests reference it in Module 6)
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(entity);
            await _repo.SaveChangesAsync();
        }

        private static CertificateTypeDto Map(CertificateType c) => new()
        {
            CertificateTypeId = c.CertificateTypeId,
            Name = c.Name,
            Description = c.Description,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt
        };
    }
}
