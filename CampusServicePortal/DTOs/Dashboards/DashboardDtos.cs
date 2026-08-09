namespace CampusServicePortal.DTOs.Dashboards;

public class StudentDashboardDto
{
    public int UpcomingLabBookings { get; set; }
    public int TotalLabReservations { get; set; }
    public int PendingComplaints { get; set; }
    public int OpenComplaints { get; set; }
    public int OutstandingFeeCount { get; set; }
    public decimal OutstandingFeeAmount { get; set; }
    public int PaidFeeCount { get; set; }
    public IReadOnlyList<DashboardComplaintItemDto> RecentComplaints { get; set; } = Array.Empty<DashboardComplaintItemDto>();
    public IReadOnlyList<DashboardFeeItemDto> RecentFees { get; set; } = Array.Empty<DashboardFeeItemDto>();
}

public class DashboardComplaintItemDto
{
    public int ComplaintId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class DashboardFeeItemDto
{
    public int FeePaymentId { get; set; }
    public string FeeTypeName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
}

public class AdminDashboardDto
{
    public int TotalRegisteredStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int PendingLabBookings { get; set; }
    public int OpenComplaints { get; set; }
    public int PendingComplaints { get; set; }
    public decimal TotalFeesAssigned { get; set; }
    public decimal TotalFeesCollected { get; set; }
    public decimal TotalFeesOutstanding { get; set; }
    public int OutstandingFeeItems { get; set; }
    public IReadOnlyList<ComplaintStatusSummaryDto> ComplaintsByCategory { get; set; } = Array.Empty<ComplaintStatusSummaryDto>();
}

public class ComplaintStatusSummaryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int Pending { get; set; }
    public int InProgress { get; set; }
    public int Resolved { get; set; }
}
