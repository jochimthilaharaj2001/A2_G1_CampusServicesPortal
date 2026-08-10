namespace CampusServicesPortal.Hostel.Services
{
    public interface INotificationQueue
    {
        Task QueueForStudentAsync(int studentId, string type, string title, string message);
        Task QueueForUserAsync(int userId, string type, string title, string message);
    }
}