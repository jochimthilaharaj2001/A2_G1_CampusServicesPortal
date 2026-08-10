using CampusServicePortal.Services.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CampusServicePortal.Services.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;


        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }


        public async Task SendVerificationEmailAsync(
            string toEmail,
            string toName,
            string token)
        {
            var frontendBaseUrl =
                _configuration["AppSettings:FrontendBaseUrl"]
                ?? "http://localhost:5500";


            var verifyLink =
                $"{frontendBaseUrl}/verify-email.html?token={token}";


            var subject =
                "Campus Services Portal - Verify Your Email";


            var htmlBody = $@"
            <html>
            <body>

            <h2 style='color:#2563eb'>
            Welcome to Campus Services Portal
            </h2>

            <p>
            Hi <b>{toName}</b>,
            </p>

            <p>
            Thank you for registering.
            Please verify your email address.
            </p>


            <a href='{verifyLink}'
            style='background:#2563eb;
            color:white;
            padding:12px 25px;
            text-decoration:none;
            border-radius:5px;'>

            Verify Email

            </a>


            <p>
            This link will expire in 24 hours.
            </p>


            </body>
            </html>
            ";


            await SendEmailAsync(
                toEmail,
                toName,
                subject,
                htmlBody
            );
        }



        public async Task SendPasswordResetEmailAsync(
            string toEmail,
            string toName,
            string token)
        {
            var frontendBaseUrl =
                _configuration["AppSettings:FrontendBaseUrl"]
                ?? "http://localhost:5500";


            var resetLink =
                $"{frontendBaseUrl}/reset-password.html?token={token}";


            var subject =
                "Campus Services Portal - Password Reset";


            var htmlBody = $@"
            <html>
            <body>

            <h2 style='color:#dc2626'>
            Password Reset Request
            </h2>


            <p>
            Hi <b>{toName}</b>
            </p>


            <p>
            Click below to reset your password.
            </p>


            <a href='{resetLink}'
            style='background:#dc2626;
            color:white;
            padding:12px 25px;
            text-decoration:none;
            border-radius:5px;'>

            Reset Password

            </a>


            <p>
            This link expires in 30 minutes.
            </p>


            </body>
            </html>
            ";


            await SendEmailAsync(
                toEmail,
                toName,
                subject,
                htmlBody
            );
        }




        private async Task SendEmailAsync(
            string toEmail,
            string toName,
            string subject,
            string htmlBody)
        {

            var apiKey = _configuration["SendGrid:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("SendGrid:ApiKey is missing from configuration.");
            }

            var fromEmail = _configuration["SendGrid:FromEmail"];
            var fromName = _configuration["SendGrid:FromName"] ?? "Campus Services Portal";
            if (string.IsNullOrWhiteSpace(fromEmail))
            {
                throw new InvalidOperationException("SendGrid:FromEmail is missing from configuration.");
            }

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var to =
                new EmailAddress(
                    toEmail,
                    toName
                );



            var message =
                MailHelper.CreateSingleEmail(
                    from,
                    to,
                    subject,
                    "",
                    htmlBody
                );



            try
            {

                var response =
                    await client.SendEmailAsync(message);



                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError(
                        "SendGrid Error Status: {Status}; Body: {Body}",
                        response.StatusCode,
                        responseBody
                    );
                    throw new Exception(
                        "SendGrid email failed"
                    );
                }

            }

            catch (Exception ex)
            {

                _logger.LogError(
                    ex,
                    "Email sending failed to {Email}",
                    toEmail
                );


                throw new InvalidOperationException(
                    "Email could not be sent",
                    ex
                );
            }

        }

    }
}