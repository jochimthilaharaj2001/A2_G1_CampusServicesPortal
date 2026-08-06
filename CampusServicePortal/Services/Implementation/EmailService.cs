using CampusServicePortal.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CampusServicePortal.Services.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendVerificationEmailAsync(string toEmail, string toName, string token)
        {
            var frontendBaseUrl = _configuration["AppSettings:FrontendBaseUrl"]
                ?? "http://localhost:5500";

            var verifyLink = $"{frontendBaseUrl}/verify-email.html?token={token}";

            var subject = "Campus Services Portal — Verify Your Email";
            var htmlBody = $@"
                <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: auto;"">
                    <h2 style=""color: #2563eb;"">Welcome to Campus Services Portal</h2>
                    <p>Hi <strong>{toName}</strong>,</p>
                    <p>Thank you for registering. Please verify your email address by clicking the button below:</p>
                    <div style=""text-align: center; margin: 30px 0;"">
                        <a href=""{verifyLink}""
                           style=""background-color: #2563eb; color: white; padding: 12px 28px;
                                  text-decoration: none; border-radius: 6px; font-size: 16px;"">
                            Verify Email Address
                        </a>
                    </div>
                    <p style=""color: #6b7280;"">This link will expire in <strong>24 hours</strong>.</p>
                    <p style=""color: #6b7280;"">If you did not register for this portal, you can safely ignore this email.</p>
                    <hr style=""border: none; border-top: 1px solid #e5e7eb;""/>
                    <p style=""color: #9ca3af; font-size: 12px;"">Campus Services Portal &mdash; University Administration System</p>
                </div>";

            await SendEmailAsync(toEmail, toName, subject, htmlBody);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string toName, string token)
        {
            var frontendBaseUrl = _configuration["AppSettings:FrontendBaseUrl"]
                ?? "http://localhost:5500";

            var resetLink = $"{frontendBaseUrl}/reset-password.html?token={token}";

            var subject = "Campus Services Portal — Password Reset Request";
            var htmlBody = $@"
                <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: auto;"">
                    <h2 style=""color: #dc2626;"">Password Reset Request</h2>
                    <p>Hi <strong>{toName}</strong>,</p>
                    <p>We received a request to reset your password. Click the button below to set a new password:</p>
                    <div style=""text-align: center; margin: 30px 0;"">
                        <a href=""{resetLink}""
                           style=""background-color: #dc2626; color: white; padding: 12px 28px;
                                  text-decoration: none; border-radius: 6px; font-size: 16px;"">
                            Reset Password
                        </a>
                    </div>
                    <p style=""color: #6b7280;"">This link will expire in <strong>30 minutes</strong>.</p>
                    <p style=""color: #6b7280;"">If you did not request a password reset, please ignore this email.
                       Your password will remain unchanged.</p>
                    <hr style=""border: none; border-top: 1px solid #e5e7eb;""/>
                    <p style=""color: #9ca3af; font-size: 12px;"">Campus Services Portal &mdash; University Administration System</p>
                </div>";

            await SendEmailAsync(toEmail, toName, subject, htmlBody);
        }

        private async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            var smtpHost = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
            var smtpUsername = _configuration["Smtp:Username"] ?? string.Empty;
            var smtpPassword = _configuration["Smtp:Password"] ?? string.Empty;
            var fromName = _configuration["Smtp:FromName"] ?? "Campus Services Portal";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, smtpUsername));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            try
            {
                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUsername, smtpPassword);
                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                // Re-throw so the caller knows the email didn't send
                throw new InvalidOperationException(
                    "Email could not be sent. Please try again later.", ex);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}
