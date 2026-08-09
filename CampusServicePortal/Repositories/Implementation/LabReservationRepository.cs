using CampusServicePortal.Data;
using CampusServicePortal.Models;
using CampusServicePortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusServicePortal.Repositories.Implementation
{
    public class LabReservationRepository
        : ILabReservationRepository
    {
        private readonly ApplicationDbContext _context;

        public LabReservationRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LabReservation>> GetAllAsync()
        {
            return await _context.LabReservations
                .Include(x => x.User)
                .Include(x => x.Lab)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<LabReservation?> GetByIdAsync(int id)
        {
            return await _context.LabReservations
                .Include(x => x.User)
                .Include(x => x.Lab)
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.LabReservationId == id);
        }

        public async Task<LabReservation> CreateAsync(
            LabReservation reservation)
        {
            await _context.LabReservations.AddAsync(reservation);

            await _context.SaveChangesAsync();

            return reservation;
        }

        public async Task<LabReservation?> UpdateAsync(
            int id,
            LabReservation reservation)
        {
            var existing =
                await _context.LabReservations
                    .FirstOrDefaultAsync(
                        x => x.LabReservationId == id);

            if (existing == null)
            {
                return null;
            }

            existing.ReservationDate =
                reservation.ReservationDate;

            existing.StartTime =
                reservation.StartTime;

            existing.EndTime =
                reservation.EndTime;

            existing.Purpose =
                reservation.Purpose;

            existing.Status =
                reservation.Status;

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var reservation =
                await _context.LabReservations
                    .FirstOrDefaultAsync(
                        x => x.LabReservationId == id);

            if (reservation == null)
            {
                return false;
            }

            _context.LabReservations.Remove(reservation);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}