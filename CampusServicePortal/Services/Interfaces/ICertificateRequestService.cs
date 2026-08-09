using CampusServicePortal.DTOs.Certificates;
namespace CampusServicePortal.Services.Interfaces;
public interface ICertificateRequestService { Task<CertificateRequestDto> CreateAsync(int userId, CreateCertificateRequestDto dto); Task<IReadOnlyList<CertificateRequestDto>> GetForUserAsync(int userId); Task<IReadOnlyList<CertificateRequestDto>> GetAllAsync(string? status); Task<CertificateRequestDto> UpdateStatusAsync(int id, UpdateCertificateRequestStatusDto dto); }
