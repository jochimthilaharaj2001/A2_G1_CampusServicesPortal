namespace CampusServicePortal.DTOs.Students
{
    /// <summary>
    /// Cross-module activity summary for a student profile (BRD Module 1).
    /// Populated as Modules 2–8 are built; empty lists until then.
    /// </summary>
    public class StudentActivitySummaryDto
    {
        public List<ActivityItemDto> HostelApplications { get; set; } = new();
        public List<ActivityItemDto> LabBookings { get; set; } = new();
        public List<ActivityItemDto> EventRegistrations { get; set; } = new();
        public List<ActivityItemDto> CertificateRequests { get; set; } = new();
        public List<ActivityItemDto> Complaints { get; set; } = new();
        public List<ActivityItemDto> FeePayments { get; set; } = new();
        public int UnreadNotifications { get; set; }
    }

    public class ActivityItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
    }
}
