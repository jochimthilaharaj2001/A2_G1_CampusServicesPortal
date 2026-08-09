namespace CampusServicePortal.DTOs.Students
{
    /// <summary>
    /// Response DTO for the StudentMasterList — returned when verifying an index number.
    /// </summary>
    public class MasterListRecordDto
    {
        public string IndexNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Faculty { get; set; } = string.Empty;
        public string DegreeProgram { get; set; } = string.Empty;
        public int EnrollmentYear { get; set; }
        public bool IsRegistered { get; set; }
    }
}
