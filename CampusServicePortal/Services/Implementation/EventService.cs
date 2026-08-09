using CampusServicePortal.Data;
using CampusServicePortal.DTOs.Events;
using CampusServicePortal.Models;
using CampusServicePortal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusServicePortal.Services.Implementation;

public class EventService(ApplicationDbContext context) : IEventService
{
    public async Task<IReadOnlyList<EventDto>> GetEventsAsync(bool admin = false) => await EventQuery(admin).OrderBy(e => e.StartsAt).ToListAsync();

    public async Task<EventDto> SaveEventAsync(int? id, SaveEventDto dto)
    {
        if (dto.StartsAt >= dto.EndsAt) throw new InvalidOperationException("The event end time must be after its start time.");
        var venue = await context.Venues.FindAsync(dto.VenueId);
        if (venue is null || !venue.IsActive) throw new InvalidOperationException("Select a valid active venue.");
        if (dto.Capacity > venue.Capacity) throw new InvalidOperationException($"This venue can host a maximum of {venue.Capacity} attendees.");
        var overlaps = await context.Events.AnyAsync(e => e.VenueId == dto.VenueId && e.IsActive && e.EventId != id && e.StartsAt < dto.EndsAt && e.EndsAt > dto.StartsAt);
        if (overlaps) throw new InvalidOperationException("This venue is already booked for the selected time range.");
        CampusEvent item;
        if (id is null) { item = new CampusEvent(); context.Events.Add(item); }
        else { item = await context.Events.FindAsync(id.Value) ?? throw new KeyNotFoundException("Event not found."); var booked = await context.EventRegistrations.CountAsync(r => r.EventId == id && r.Status == "Confirmed"); if (dto.Capacity < booked) throw new InvalidOperationException("Capacity cannot be lower than confirmed registrations."); }
        item.Title = dto.Title.Trim(); item.Description = dto.Description?.Trim(); item.VenueId = dto.VenueId; item.StartsAt = dto.StartsAt; item.EndsAt = dto.EndsAt; item.Capacity = dto.Capacity; item.UsesReservedSeating = dto.UsesReservedSeating; item.IsActive = dto.IsActive; item.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(); return await EventQuery(true).FirstAsync(e => e.EventId == item.EventId);
    }
    public async Task DeactivateEventAsync(int id) { var item = await context.Events.FindAsync(id) ?? throw new KeyNotFoundException("Event not found."); item.IsActive = false; item.UpdatedAt = DateTime.UtcNow; await context.SaveChangesAsync(); }
    public async Task<IReadOnlyList<VenueDto>> GetVenuesAsync(bool includeInactive = false) => await context.Venues.AsNoTracking().Where(v => includeInactive || v.IsActive).OrderBy(v => v.Name).Select(v => ToVenue(v)).ToListAsync();
    public async Task<VenueDto> SaveVenueAsync(int? id, SaveVenueDto dto)
    {
        var name = dto.Name.Trim(); if (await context.Venues.AnyAsync(v => v.Name == name && v.VenueId != id)) throw new InvalidOperationException("A venue with this name already exists.");
        Venue item; if (id is null) { item = new Venue(); context.Venues.Add(item); } else { item = await context.Venues.FindAsync(id.Value) ?? throw new KeyNotFoundException("Venue not found."); var maxEventCapacity = await context.Events.Where(e => e.VenueId == id && e.IsActive).Select(e => (int?)e.Capacity).MaxAsync() ?? 0; if (dto.Capacity < maxEventCapacity) throw new InvalidOperationException("Venue capacity cannot be lower than an active event's capacity."); }
        item.Name = name; item.VenueType = dto.VenueType; item.Capacity = dto.Capacity; item.Location = dto.Location?.Trim(); item.IsActive = dto.IsActive; await context.SaveChangesAsync(); return ToVenue(item);
    }
    public async Task DeactivateVenueAsync(int id) { var item = await context.Venues.FindAsync(id) ?? throw new KeyNotFoundException("Venue not found."); if (await context.Events.AnyAsync(e => e.VenueId == id && e.IsActive && e.EndsAt > DateTime.UtcNow)) throw new InvalidOperationException("This venue has upcoming active events and cannot be deactivated."); item.IsActive = false; await context.SaveChangesAsync(); }
    public async Task<IReadOnlyList<EventSeatDto>> GetSeatsAsync(int eventId) => await context.EventSeats.AsNoTracking().Where(s => s.EventId == eventId && s.IsActive).OrderBy(s => s.SeatNumber).Select(s => new EventSeatDto { EventSeatId = s.EventSeatId, SeatNumber = s.SeatNumber, IsAvailable = !context.EventRegistrations.Any(r => r.EventSeatId == s.EventSeatId && r.Status == "Confirmed") }).ToListAsync();
    public async Task<EventSeatDto> AddSeatAsync(int eventId, CreateEventSeatDto dto)
    {
        var item = await context.Events.FindAsync(eventId) ?? throw new KeyNotFoundException("Event not found."); if (!item.UsesReservedSeating) throw new InvalidOperationException("Enable reserved seating before configuring seats."); var count = await context.EventSeats.CountAsync(s => s.EventId == eventId && s.IsActive); if (count >= item.Capacity) throw new InvalidOperationException("Seat count cannot exceed event capacity."); var number = dto.SeatNumber.Trim(); if (await context.EventSeats.AnyAsync(s => s.EventId == eventId && s.SeatNumber == number)) throw new InvalidOperationException("This seat number already exists."); var seat = new EventSeat { EventId = eventId, SeatNumber = number }; context.EventSeats.Add(seat); await context.SaveChangesAsync(); return new EventSeatDto { EventSeatId = seat.EventSeatId, SeatNumber = seat.SeatNumber, IsAvailable = true };
    }
    public async Task RemoveSeatAsync(int eventId, int seatId) { var seat = await context.EventSeats.FirstOrDefaultAsync(s => s.EventId == eventId && s.EventSeatId == seatId) ?? throw new KeyNotFoundException("Seat not found."); if (await context.EventRegistrations.AnyAsync(r => r.EventSeatId == seatId && r.Status == "Confirmed")) throw new InvalidOperationException("A booked seat cannot be removed."); context.EventSeats.Remove(seat); await context.SaveChangesAsync(); }
    public async Task<EventRegistrationDto> RegisterAsync(int userId, CreateEventRegistrationDto dto)
    {
        var item = await context.Events.Include(e => e.EventSeats).FirstOrDefaultAsync(e => e.EventId == dto.EventId) ?? throw new KeyNotFoundException("Event not found."); if (!item.IsActive || item.StartsAt <= DateTime.UtcNow) throw new InvalidOperationException("Registration is closed for this event."); if (await context.EventRegistrations.AnyAsync(r => r.EventId == dto.EventId && r.UserId == userId && r.Status == "Confirmed")) throw new InvalidOperationException("You are already registered for this event."); var count = await context.EventRegistrations.CountAsync(r => r.EventId == dto.EventId && r.Status == "Confirmed"); if (count >= item.Capacity) throw new InvalidOperationException("This event has reached its capacity.");
        EventSeat? seat = null; if (item.UsesReservedSeating) { if (item.EventSeats.Count(s => s.IsActive) != item.Capacity) throw new InvalidOperationException("Registration opens after all reserved seats are configured."); if (!dto.EventSeatId.HasValue) throw new InvalidOperationException("Select a seat for this event."); seat = item.EventSeats.FirstOrDefault(s => s.EventSeatId == dto.EventSeatId && s.IsActive) ?? throw new InvalidOperationException("Select a valid seat."); if (await context.EventRegistrations.AnyAsync(r => r.EventSeatId == seat.EventSeatId && r.Status == "Confirmed")) throw new InvalidOperationException("This seat has already been booked."); } else if (dto.EventSeatId.HasValue) throw new InvalidOperationException("This event does not use reserved seating.");
        var registration = new EventRegistration { EventId = item.EventId, UserId = userId, EventSeatId = seat?.EventSeatId }; context.EventRegistrations.Add(registration); await context.SaveChangesAsync(); return await RegistrationQuery().FirstAsync(r => r.EventRegistrationId == registration.EventRegistrationId);
    }
    public async Task<IReadOnlyList<EventRegistrationDto>> GetRegistrationsAsync(int userId) => await RegistrationQuery().Where(r => r.UserId == userId).OrderBy(r => r.StartsAt).ToListAsync();
    public async Task CancelRegistrationAsync(int id, int userId, bool isAdmin) { var item = await context.EventRegistrations.FindAsync(id) ?? throw new KeyNotFoundException("Event registration not found."); if (!isAdmin && item.UserId != userId) throw new UnauthorizedAccessException(); if (item.Status != "Confirmed") throw new InvalidOperationException("This registration is already cancelled."); item.Status = "Cancelled"; item.CancelledAt = DateTime.UtcNow; await context.SaveChangesAsync(); }
    private IQueryable<EventDto> EventQuery(bool admin) => context.Events.AsNoTracking().Include(e => e.Venue).Include(e => e.EventSeats).Include(e => e.EventRegistrations).Where(e => admin || (e.IsActive && e.EndsAt >= DateTime.UtcNow)).Select(e => new EventDto { EventId = e.EventId, Title = e.Title, Description = e.Description, VenueId = e.VenueId, VenueName = e.Venue!.Name, VenueType = e.Venue.VenueType, VenueCapacity = e.Venue.Capacity, StartsAt = e.StartsAt, EndsAt = e.EndsAt, Capacity = e.Capacity, RegisteredCount = e.EventRegistrations.Count(r => r.Status == "Confirmed"), AvailableSlots = e.Capacity - e.EventRegistrations.Count(r => r.Status == "Confirmed"), UsesReservedSeating = e.UsesReservedSeating, IsRegistrationOpen = e.IsActive && e.StartsAt > DateTime.UtcNow && (!e.UsesReservedSeating || e.EventSeats.Count(s => s.IsActive) == e.Capacity), IsActive = e.IsActive });
    private IQueryable<EventRegistrationDto> RegistrationQuery() => context.EventRegistrations.AsNoTracking().Include(r => r.Event).ThenInclude(e => e!.Venue).Include(r => r.Seat).Select(r => new EventRegistrationDto { EventRegistrationId = r.EventRegistrationId, UserId = r.UserId, EventId = r.EventId, EventTitle = r.Event!.Title, VenueName = r.Event.Venue!.Name, StartsAt = r.Event.StartsAt, EndsAt = r.Event.EndsAt, SeatNumber = r.Seat != null ? r.Seat.SeatNumber : null, Status = r.Status, RegisteredAt = r.RegisteredAt });
    private static VenueDto ToVenue(Venue v) => new() { VenueId = v.VenueId, Name = v.Name, VenueType = v.VenueType, Capacity = v.Capacity, Location = v.Location, IsActive = v.IsActive };
}
