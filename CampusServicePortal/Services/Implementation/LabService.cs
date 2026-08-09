using CampusServicePortal.Data;
using CampusServicePortal.DTOs.Labs;
using CampusServicePortal.Models;
using CampusServicePortal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusServicePortal.Services.Implementation
{
    public class LabService : ILabService
    {
        private readonly ApplicationDbContext _context;

        public LabService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LabDto>> GetAllLabsAsync()
        {
            return await _context.Labs
                .Select(l => new LabDto
                {
                    LabId = l.LabId,
                    Name = l.Name,
                    RoomNumber = l.RoomNumber,
                    IsActive = l.IsActive,
                    LabType = l.LabType,
                    Capacity = l.Capacity,
                    ActiveSeatCount = _context.LabSeats.Count(s => s.LabId == l.LabId && s.IsActive)
                })
                .ToListAsync();
        }

        public async Task<LabDto?> GetLabByIdAsync(int id)
        {
            var l = await _context.Labs.FindAsync(id);
            if (l == null) return null;

            return new LabDto
            {
                LabId = l.LabId,
                Name = l.Name,
                RoomNumber = l.RoomNumber,
                IsActive = l.IsActive,
                LabType = l.LabType,
                Capacity = l.Capacity,
                ActiveSeatCount = await _context.LabSeats.CountAsync(s => s.LabId == l.LabId && s.IsActive)
            };
        }

        public async Task<LabDto> CreateLabAsync(CreateLabDto dto)
        {
            var lab = new Lab
            {
                Name = dto.Name,
                RoomNumber = dto.RoomNumber,
                LabType = dto.LabType,
                Capacity = dto.Capacity,
                IsActive = true
            };

            await _context.Labs.AddAsync(lab);
            await _context.SaveChangesAsync();

            return new LabDto
            {
                LabId = lab.LabId,
                Name = lab.Name,
                RoomNumber = lab.RoomNumber,
                IsActive = lab.IsActive,
                LabType = lab.LabType,
                Capacity = lab.Capacity
            };
        }

        public async Task<LabDto?> UpdateLabAsync(int id, UpdateLabDto dto)
        {
            var lab = await _context.Labs.FindAsync(id);
            if (lab == null) return null;

            lab.Name = dto.Name;
            lab.RoomNumber = dto.RoomNumber;
            lab.Capacity = dto.Capacity;
            lab.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return new LabDto
            {
                LabId = lab.LabId,
                Name = lab.Name,
                RoomNumber = lab.RoomNumber,
                IsActive = lab.IsActive,
                LabType = lab.LabType,
                Capacity = lab.Capacity,
                ActiveSeatCount = await _context.LabSeats.CountAsync(s => s.LabId == lab.LabId && s.IsActive)
            };
        }

        public async Task<bool> DeleteLabAsync(int id)
        {
            var lab = await _context.Labs.FindAsync(id);
            if (lab == null) return false;

            lab.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<LabSeatDto?> AddSeatAsync(int labId, CreateLabSeatDto dto)
        {
            var lab = await _context.Labs.FindAsync(labId);
            if (lab == null || lab.LabType != LabType.Computer)
            {
                throw new InvalidOperationException("Seats can only be added to Computer Labs.");
            }

            var exists = await _context.LabSeats.AnyAsync(s => s.LabId == labId && s.SeatNumber == dto.SeatNumber);
            if (exists)
            {
                throw new InvalidOperationException("Seat number already exists in this lab.");
            }

            var seat = new LabSeat
            {
                LabId = labId,
                SeatNumber = dto.SeatNumber,
                IsActive = true
            };

            await _context.LabSeats.AddAsync(seat);
            await _context.SaveChangesAsync();

            return new LabSeatDto
            {
                SeatId = seat.SeatId,
                LabId = seat.LabId,
                SeatNumber = seat.SeatNumber,
                IsActive = seat.IsActive
            };
        }

        public async Task<bool> RemoveSeatAsync(int labId, int seatId)
        {
            var seat = await _context.LabSeats.FirstOrDefaultAsync(s => s.SeatId == seatId && s.LabId == labId);
            if (seat == null) return false;

            // Business Rule: A seat cannot be deleted while it has future active bookings.
            var today = DateTime.UtcNow.Date;
            var hasFutureBookings = await _context.LabReservations
                .AnyAsync(r => r.SeatId == seatId && r.ReservationDate >= today && r.Status != "Cancelled");

            if (hasFutureBookings)
            {
                throw new InvalidOperationException("Cannot delete seat with future active bookings.");
            }

            _context.LabSeats.Remove(seat);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<SeatAvailabilityDto>> GetSeatAvailabilityAsync(int labId, DateTime date, TimeSpan slotStartTime)
        {
            var lab = await _context.Labs.FindAsync(labId);
            if (lab == null || lab.LabType != LabType.Computer)
            {
                return Enumerable.Empty<SeatAvailabilityDto>();
            }

            var seats = await _context.LabSeats
                .Where(s => s.LabId == labId && s.IsActive)
                .ToListAsync();

            var slotEndTime = slotStartTime.Add(TimeSpan.FromHours(1));

            var bookedSeats = await _context.LabReservations
                .Where(r => r.LabId == labId 
                            && r.ReservationDate.Date == date.Date 
                            && r.Status != "Cancelled"
                            && r.StartTime < slotEndTime 
                            && r.EndTime > slotStartTime)
                .Select(r => r.SeatId)
                .ToListAsync();

            return seats.Select(s => new SeatAvailabilityDto
            {
                SeatId = s.SeatId,
                SeatNumber = s.SeatNumber,
                IsBooked = bookedSeats.Contains(s.SeatId)
            });
        }

        public async Task<IEnumerable<TimeSpan>> GetAvailableSlotsAsync(int labId, DateTime date)
        {
            var lab = await _context.Labs.FindAsync(labId);
            if (lab == null || !lab.IsActive) return Enumerable.Empty<TimeSpan>();

            // Define standard slots from 9:00 to 17:00 hourly
            var allSlots = new List<TimeSpan>
            {
                TimeSpan.FromHours(9),
                TimeSpan.FromHours(10),
                TimeSpan.FromHours(11),
                TimeSpan.FromHours(12),
                TimeSpan.FromHours(13),
                TimeSpan.FromHours(14),
                TimeSpan.FromHours(15),
                TimeSpan.FromHours(16)
            };

            var availableSlots = new List<TimeSpan>();

            foreach (var slot in allSlots)
            {
                var slotEndTime = slot.Add(TimeSpan.FromHours(1));

                var overlappingBookings = await _context.LabReservations
                    .Where(r => r.LabId == labId 
                                && r.ReservationDate.Date == date.Date 
                                && r.Status != "Cancelled"
                                && r.StartTime < slotEndTime 
                                && r.EndTime > slot)
                    .ToListAsync();

                if (lab.LabType == LabType.Science)
                {
                    // Available if count of bookings is less than overall capacity
                    if (overlappingBookings.Count < lab.Capacity)
                    {
                        availableSlots.Add(slot);
                    }
                }
                else // Computer Lab
                {
                    // Available if there is at least one active seat not booked
                    var totalActiveSeatsCount = await _context.LabSeats.CountAsync(s => s.LabId == labId && s.IsActive);
                    var bookedSeatsCount = overlappingBookings.Count(r => r.SeatId.HasValue);
                    if (bookedSeatsCount < totalActiveSeatsCount)
                    {
                        availableSlots.Add(slot);
                    }
                }
            }

            return availableSlots;
        }
    }
}
