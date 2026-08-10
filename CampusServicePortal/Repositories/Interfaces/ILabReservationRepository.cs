using CampusServicePortal.Models;

namespace CampusServicePortal.Repositories.Interfaces
{
    public interface ILabReservationRepository
    {
        Task<IEnumerable<LabReservation>> GetAllAsync();

        Task<LabReservation?> GetByIdAsync(int id);

        Task<LabReservation> CreateAsync(
            LabReservation reservation);

        Task<LabReservation?> UpdateAsync(
            int id,
            LabReservation reservation);

        Task<bool> DeleteAsync(int id);
    }
}