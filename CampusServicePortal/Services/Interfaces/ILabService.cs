using CampusServicePortal.DTOs.Labs;

namespace CampusServicePortal.Services.Interfaces
{
    public interface ILabService
    {
        Task<IEnumerable<LabDto>> GetAllLabsAsync();
        Task<LabDto?> GetLabByIdAsync(int id);
        Task<LabDto> CreateLabAsync(CreateLabDto dto);
        Task<LabDto?> UpdateLabAsync(int id, UpdateLabDto dto);
        Task<bool> DeleteLabAsync(int id);

        Task<LabSeatDto?> AddSeatAsync(int labId, CreateLabSeatDto dto);
        Task<bool> RemoveSeatAsync(int labId, int seatId);
        Task<IEnumerable<SeatAvailabilityDto>> GetSeatAvailabilityAsync(int labId, DateTime date, TimeSpan slotStartTime);

        Task<IEnumerable<TimeSpan>> GetAvailableSlotsAsync(int labId, DateTime date);
    }
}
