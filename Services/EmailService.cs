using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Mail;

namespace Newsletter_Backend_Function.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        // Confirmation Emailservice

        public async Task SendConfirmationEmail(string toEmail, string confirmationToken, string unsubscribeToken)
        {
            var templatePath = Path.Combine(
                AppContext.BaseDirectory,
                "Templates",
                "ConfirmationEmail.html"
            );

            var htmlContent = await File.ReadAllTextAsync(templatePath);

            var apiBaseUrl = _configuration["App:ApiBaseUrl"];

            var confirmLink =
                $"{apiBaseUrl}/api/subscribers/confirm?token={confirmationToken}";

            var unsubscribeLink =
                $"{apiBaseUrl}/api/subscribers/unsubscribe?token={unsubscribeToken}";

            htmlContent = htmlContent
                .Replace("{{CONFIRM_LINK}}", confirmLink)
                .Replace("{{UNSUBSCRIBE_LINK}}", unsubscribeLink);

            var plainTextContent =
                $"Bitte bestätige deine Anmeldung: {confirmLink}\n\n" +
                $"Falls du dich nicht angemeldet hast, ignoriere diese E-Mail.";

            await SendEmail(
                toEmail,
                "Bitte bestätige deine Anmeldung",
                htmlContent,
                plainTextContent
            );
        }


        // Welcome Emailservice

        public async Task SendWelcomeEmail(string toEmail, string unsubscribeToken)
        {
            var templatePath = Path.Combine(
                AppContext.BaseDirectory,
                "Templates",
                "WelcomeEmail.html"
            );

            var htmlContent = await File.ReadAllTextAsync(templatePath);

            var apiBaseUrl = _configuration["App:ApiBaseUrl"];

            var unsubscribeLink =
                $"{apiBaseUrl}/api/subscribers/unsubscribe?token={unsubscribeToken}";

            htmlContent = htmlContent
                .Replace("{{UNSUBSCRIBE_LINK}}", unsubscribeLink);

            var plainTextContent =
                "Willkommen! Deine Anmeldung zum Newsletter war erfolgreich.";

            await SendEmail(
                toEmail,
                "Willkommen im Newsletter",
                htmlContent,
                plainTextContent
            );
        }


        // Sendgrid Versendelogik

        private async Task SendEmail(
            string toEmail,
            string subject,
            string htmlContent,
            string plainTextContent)
        {
            var apiKey = _configuration["SendGrid:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new Exception("SendGrid API Key fehlt in der Konfiguration.");
            }

            var client = new SendGridClient(apiKey);

            var from = new EmailAddress("info@mschott.dev", "Newsletter");
            var to = new EmailAddress(toEmail);

            var msg = MailHelper.CreateSingleEmail(
                from,
                to,
                subject,
                plainTextContent,
                htmlContent
            );

            var response = await client.SendEmailAsync(msg);

            var body = await response.Body.ReadAsStringAsync();

            Console.WriteLine($"===== SENDGRID =====");
            Console.WriteLine($"Status: {(int)response.StatusCode}");
            Console.WriteLine($"Body: {body}");
            Console.WriteLine("====================");

            if ((int)response.StatusCode < 200 || (int)response.StatusCode >= 300)
            {
                throw new Exception(
                    $"SendGrid Fehler {(int)response.StatusCode}: {body}"
                );
            }
        }
    }
}