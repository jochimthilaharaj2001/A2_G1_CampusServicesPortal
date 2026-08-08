using CampusServicePortal.Data;
using CampusServicePortal.DTOs.LabReservation;
using CampusServicePortal.Models;
using CampusServicePortal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusServicePortal.Services.Implementation
{
    public class LabReservationService : ILabReservationService
    {
        private readonly ApplicationDbContext _context;

        public LabReservationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LabReservationDTO>> GetAllAsync()
        {
            var reservations = await _context.LabReservations
                .Include(x => x.User)
                .Include(x => x.Lab)
                .Include(x => x.Seat)
                .AsNoTracking()
                .ToListAsync();

            return reservations.Select(x => MapToDTO(x));
        }

        public async Task<LabReservationDTO?> GetByIdAsync(int id)
        {
            var reservation = await _context.LabReservations
                .Include(x => x.User)
                .Include(x => x.Lab)
                .Include(x => x.Seat)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.LabReservationId == id);

            if (reservation == null) return null;

            return MapToDTO(reservation);
        }

        public async Task<LabReservationDTO> CreateAsync(CreateLabReservationDTO dto)
        {
            var lab = await _context.Labs.FindAsync(dto.LabId);
            if (lab == null || !lab.IsActive)
            {
                throw new InvalidOperationException("The selected lab does not exist or is inactive.");
            }

            if (dto.StartTime >= dto.EndTime)
            {
                throw new InvalidOperationException("Start time must be before end time.");
            }

            // Check validations based on LabType
            if (lab.LabType == LabType.Computer)
            {
                if (!dto.SeatId.HasValue)
                {
                    throw new InvalidOperationException("A seat must be selected for Computer Labs.");
                }

                var seat = await _context.LabSeats.FirstOrDefaultAsync(s => s.SeatId == dto.SeatId.Value && s.LabId == dto.LabId);
                if (seat == null || !seat.IsActive)
                {
                    throw new InvalidOperationException("The selected seat is invalid or inactive.");
                }

                // Check for double booking of the seat
                var isSeatBooked = await _context.LabReservations
                    .AnyAsync(r => r.SeatId == dto.SeatId.Value 
                                && r.ReservationDate.Date == dto.ReservationDate.Date 
                                && r.Status != "Cancelled"
                                && r.StartTime < dto.EndTime 
                                && r.EndTime > dto.StartTime);

                if (isSeatBooked)
                {
                    throw new InvalidOperationException("This seat is already reserved for the selected time slot.");
                }
            }
            else // Science Lab
            {
                // Verify seat is not selected
                if (dto.SeatId.HasValue)
                {
                    throw new InvalidOperationException("Seats cannot be reserved in Science Labs.");
                }

                // Count active overlapping bookings for this science lab slot
                var currentBookingsCount = await _context.LabReservations
                    .CountAsync(r => r.LabId == dto.LabId 
                                && r.ReservationDate.Date == dto.ReservationDate.Date 
                                && r.Status != "Cancelled"
                                && r.StartTime < dto.EndTime 
                                && r.EndTime > dto.StartTime);

                if (currentBookingsCount >= lab.Capacity)
                {
                    throw new InvalidOperationException("The science lab has reached its capacity for the selected time slot.");
                }
            }

            var reservation = new LabReservation
            {
                UserId = dto.UserId,
                LabId = dto.LabId,
                SeatId = dto.SeatId,
                ReservationDate = dto.ReservationDate,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Purpose = dto.Purpose,
                Status = "Approved", // Automatically approved
                CreatedAt = DateTime.UtcNow
            };

            await _context.LabReservations.AddAsync(reservation);
            await _context.SaveChangesAsync();

            // Fetch relations for mapping
            await _context.Entry(reservation).Reference(r => r.User).LoadAsync();
            await _context.Entry(reservation).Reference(r => r.Lab).LoadAsync();
            if (reservation.SeatId.HasValue)
            {
                await _context.Entry(reservation).Reference(r => r.Seat).LoadAsync();
            }

            return MapToDTO(reservation);
        }

        public async Task<LabReservationDTO?> UpdateAsync(int id, UpdateLabReservationDTO dto)
        {
            var existing = await _context.LabReservations
                .Include(r => r.Lab)
                .FirstOrDefaultAsync(r => r.LabReservationId == id);

            if (existing == null) return null;

            if (dto.StartTime >= dto.EndTime)
            {
                throw new InvalidOperationException("Start time must be before end time.");
            }

            // If reservation date/time/seat changed, re-validate capacity/double booking
            bool isTimeOrSeatChanged = existing.ReservationDate.Date != dto.ReservationDate.Date 
                                      || existing.StartTime != dto.StartTime 
                                      || existing.EndTime != dto.EndTime;

            if (isTimeOrSeatChanged)
            {
                if (existing.Lab!.LabType == LabType.Computer)
                {
                    var isSeatBooked = await _context.LabReservations
                        .AnyAsync(r => r.LabReservationId != id
                                    && r.SeatId == existing.SeatId 
                                    && r.ReservationDate.Date == dto.ReservationDate.Date 
                                    && r.Status != "Cancelled"
                                    && r.StartTime < dto.EndTime 
                                    && r.EndTime > dto.StartTime);

                    if (isSeatBooked)
                    {
                        throw new InvalidOperationException("This seat is already reserved for the selected time slot.");
                    }
                }
                else // Science Lab
                {
                    var currentBookingsCount = await _context.LabReservations
                        .CountAsync(r => r.LabReservationId != id
                                    && r.LabId == existing.LabId 
                                    && r.ReservationDate.Date == dto.ReservationDate.Date 
                                    && r.Status != "Cancelled"
                                    && r.StartTime < dto.EndTime 
                                    && r.EndTime > dto.StartTime);

                    if (currentBookingsCount >= existing.Lab.Capacity)
                    {
                        throw new InvalidOperationException("The science lab has reached its capacity for the selected time slot.");
                    }
                }
            }

            existing.ReservationDate = dto.ReservationDate;
            existing.StartTime = dto.StartTime;
            existing.EndTime = dto.EndTime;
            existing.Purpose = dto.Purpose;
            existing.Status = dto.Status;

            await _context.SaveChangesAsync();

            // Reload user and seat details
            await _context.Entry(existing).Reference(r => r.User).LoadAsync();
            if (existing.SeatId.HasValue)
            {
                await _context.Entry(existing).Reference(r => r.Seat).LoadAsync();
            }

            return MapToDTO(existing);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var reservation = await _context.LabReservations.FindAsync(id);
            if (reservation == null) return false;

            _context.LabReservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<LabReservationDTO>> GetByStudentIdAsync(int studentId)
        {
            // Map studentId (which refers to Student entity ID) to its corresponding User ID.
            var student = await _context.Students.FindAsync(studentId);
            if (student == null) return Enumerable.Empty<LabReservationDTO>();

            var reservations = await _context.LabReservations
                .Include(x => x.User)
                .Include(x => x.Lab)
                .Include(x => x.Seat)
                .Where(x => x.UserId == student.UserId)
                .AsNoTracking()
                .ToListAsync();

            return reservations.Select(x => MapToDTO(x));
        }

        public async Task<bool> CancelReservationAsync(int id, int userId, string userRole)
        {
            var reservation = await _context.LabReservations.FindAsync(id);
            if (reservation == null) return false;

            // Business Rule: A student may only cancel their own reservation, not another student's. (Admin can cancel any)
            if (userRole != "Admin" && reservation.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to cancel this reservation.");
            }

            reservation.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return true;
        }

        private static LabReservationDTO MapToDTO(LabReservation x)
        {
            return new LabReservationDTO
            {
                LabReservationId = x.LabReservationId,
                UserId = x.UserId,
                UserName = x.User?.Username,
                LabId = x.LabId,
                LabName = x.Lab?.Name,
                SeatId = x.SeatId,
                SeatNumber = x.Seat?.SeatNumber,
                ReservationDate = x.ReservationDate,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                Purpose = x.Purpose,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            };
        }
    }
}