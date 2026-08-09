using CampusServicePortal.Data;
using CampusServicePortal.DTOs.Dashboards;
using CampusServicePortal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusServicePortal.Services.Implementation;

public class DashboardService(ApplicationDbContext context) : IDashboardService
{
    public async Task<StudentDashboardDto> GetStudentDashboardAsync(int userId)
    {
        var now = DateTime.UtcNow;
        var complaints = context.Complaints.AsNoTracking().Where(c => c.UserId == userId);
        var fees = context.FeePayments.AsNoTracking().Where(f => f.UserId == userId);
        var labs = context.LabReservations.AsNoTracking().Where(l => l.UserId == userId);
        return new StudentDashboardDto
        {
            UpcomingLabBookings = await labs.CountAsync(l => l.ReservationDate >= now.Date && l.Status != "Cancelled"),
            TotalLabReservations = await labs.CountAsync(),
            PendingComplaints = await complaints.CountAsync(c => c.Status == "Pending"),
            OpenComplaints = await complaints.CountAsync(c => c.Status != "Resolved"),
            OutstandingFeeCount = await fees.CountAsync(f => f.Status == "Outstanding"),
            OutstandingFeeAmount = await fees.Where(f => f.Status == "Outstanding").SumAsync(f => (decimal?)f.Amount) ?? 0,
            PaidFeeCount = await fees.CountAsync(f => f.Status == "Paid"),
            RecentComplaints = await complaints.Include(c => c.Category).OrderByDescending(c => c.CreatedAt).Take(4)
                .Select(c => new DashboardComplaintItemDto { ComplaintId = c.ComplaintId, CategoryName = c.Category!.Name, Status = c.Status, CreatedAt = c.CreatedAt }).ToListAsync(),
            RecentFees = await fees.Include(f => f.FeeType).Where(f => f.Status != "Cancelled").OrderByDescending(f => f.AssignedAt).Take(4)
                .Select(f => new DashboardFeeItemDto { FeePaymentId = f.FeePaymentId, FeeTypeName = f.FeeType!.Name, Amount = f.Amount, Status = f.Status, BillingPeriod = f.BillingPeriod }).ToListAsync()
        };
    }

    public async Task<AdminDashboardDto> GetAdminDashboardAsync()
    {
        var payments = context.FeePayments.AsNoTracking();
        var complaints = context.Complaints.AsNoTracking();
        return new AdminDashboardDto
        {
            TotalRegisteredStudents = await context.Students.CountAsync(),
            ActiveStudents = await context.Students.CountAsync(s => s.IsActive),
            PendingLabBookings = await context.LabReservations.CountAsync(l => l.Status == "Pending"),
            OpenComplaints = await complaints.CountAsync(c => c.Status != "Resolved"),
            PendingComplaints = await complaints.CountAsync(c => c.Status == "Pending"),
            TotalFeesAssigned = await payments.Where(f => f.Status != "Cancelled").SumAsync(f => (decimal?)f.Amount) ?? 0,
            TotalFeesCollected = await payments.Where(f => f.Status == "Paid").SumAsync(f => (decimal?)f.Amount) ?? 0,
            TotalFeesOutstanding = await payments.Where(f => f.Status == "Outstanding").SumAsync(f => (decimal?)f.Amount) ?? 0,
            OutstandingFeeItems = await payments.CountAsync(f => f.Status == "Outstanding"),
            ComplaintsByCategory = await complaints.Include(c => c.Category).GroupBy(c => c.Category!.Name).OrderBy(g => g.Key)
                .Select(g => new ComplaintStatusSummaryDto { CategoryName = g.Key, Pending = g.Count(c => c.Status == "Pending"), InProgress = g.Count(c => c.Status == "In Progress"), Resolved = g.Count(c => c.Status == "Resolved") }).ToListAsync()
        };
    }
}
