using CampusServicePortal.DTOs.LabReservation;

namespace CampusServicePortal.Services.Interfaces
{
    public interface ILabReservationService
    {
        Task<IEnumerable<LabReservationDTO>> GetAllAsync();

        Task<LabReservationDTO?> GetByIdAsync(int id);

        Task<LabReservationDTO> CreateAsync(
            CreateLabReservationDTO dto);

        Task<LabReservationDTO?> UpdateAsync(
            int id,
            UpdateLabReservationDTO dto);

        Task<bool> DeleteAsync(int id);

        Task<IEnumerable<LabReservationDTO>> GetByStudentIdAsync(int studentId);

        Task<bool> CancelReservationAsync(int id, int userId, string userRole);
    }
}