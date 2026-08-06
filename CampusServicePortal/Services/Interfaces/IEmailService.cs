namespace CampusServicePortal.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string toEmail, string toName, string token);
        Task SendPasswordResetEmailAsync(string toEmail, string toName, string token);
    }
}
