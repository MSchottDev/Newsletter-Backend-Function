using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Newsletter_Backend_Function.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        // =========================================================
        // Confirmation Emailservice
        // =========================================================

        public async Task SendConfirmationEmail(
            string toEmail,
            string confirmationToken,
            string unsubscribeToken)
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


        // =========================================================
        // Welcome Emailservice
        // =========================================================

        public async Task SendWelcomeEmail(
            string toEmail,
            string unsubscribeToken)
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


        // =========================================================
        // Newsletter Emailservice
        // =========================================================

        public async Task SendNewsletterEmail(
            string toEmail,
            string unsubscribeToken,
            string title,
            string repoName,
            string description,
            string repoLink)
        {
            var templatePath = Path.Combine(
                AppContext.BaseDirectory,
                "Templates",
                "NewsletterEmail.html"
            );

            var htmlContent = await File.ReadAllTextAsync(templatePath);

            var apiBaseUrl = _configuration["App:ApiBaseUrl"];

            var unsubscribeLink =
                $"{apiBaseUrl}/api/subscribers/unsubscribe?token={unsubscribeToken}";

            htmlContent = htmlContent
                .Replace("{{TITLE}}", title)
                .Replace("{{REPO_NAME}}", repoName)
                .Replace("{{DESCRIPTION}}", description)
                .Replace("{{REPO_LINK}}", repoLink)
                .Replace("{{UNSUBSCRIBE_LINK}}", unsubscribeLink);

            var plainTextContent =
                $"Hallo,\n\n" +
                $"es gibt Neuigkeiten von Matthias Schott.\n\n" +
                $"Ich habe ein neues GitHub-Projekt gestartet: {title}\n\n" +
                $"Repository: {repoName}\n\n" +
                $"{description}\n\n" +
                $"Zum GitHub Repository:\n{repoLink}\n\n" +
                $"Newsletter abbestellen:\n{unsubscribeLink}";

            await SendEmail(
                toEmail,
                title,
                htmlContent,
                plainTextContent
            );
        }


        // =========================================================
        // Contact Emailservice
        // =========================================================

        public async Task SendContactEmail(
            string name,
            string email,
            string message)
        {
            var subject =
                $"Neue Kontaktanfrage von {name}";

            var encodedName =
                System.Net.WebUtility.HtmlEncode(name);

            var encodedEmail =
                System.Net.WebUtility.HtmlEncode(email);

            var encodedMessage =
                System.Net.WebUtility.HtmlEncode(message)
                    .Replace("\r\n", "<br>")
                    .Replace("\n", "<br>");

            var htmlContent = $@"
<!DOCTYPE html>

<html lang=""de"">

<head>
    <meta charset=""UTF-8"">
</head>

<body style=""
    margin:0;
    padding:30px;
    background-color:#0d1117;
    font-family:Arial,Helvetica,sans-serif;
    color:#f0f6fc;"">

    <table
        role=""presentation""
        width=""100%""
        cellspacing=""0""
        cellpadding=""0"">

        <tr>
            <td align=""center"">

                <table
                    role=""presentation""
                    width=""600""
                    cellspacing=""0""
                    cellpadding=""0""
                    style=""
                        max-width:92%;
                        background-color:#161b22;
                        border:1px solid #30363d;
                        border-radius:14px;
                        padding:30px;"">

                    <tr>
                        <td>

                            <h1 style=""
                                margin:0 0 25px;
                                color:#58a6fa;
                                font-size:24px;"">

                                Neue Kontaktanfrage

                            </h1>


                            <p style=""
                                color:#c9d1d9;
                                line-height:1.7;"">

                                Über das Kontaktformular von
                                <strong>mschott.dev</strong>
                                wurde eine neue Nachricht gesendet.

                            </p>


                            <hr style=""
                                border:0;
                                border-top:1px solid #30363d;
                                margin:25px 0;"">


                            <p style=""
                                color:#c9d1d9;
                                line-height:1.7;"">

                                <strong style=""color:#58a6fa;"">
                                    Name
                                </strong>
                                <br>

                                {encodedName}

                            </p>


                            <p style=""
                                color:#c9d1d9;
                                line-height:1.7;"">

                                <strong style=""color:#58a6fa;"">
                                    E-Mail
                                </strong>
                                <br>

                                {encodedEmail}

                            </p>


                            <p style=""
                                color:#c9d1d9;
                                line-height:1.7;"">

                                <strong style=""color:#58a6fa;"">
                                    Nachricht
                                </strong>
                                <br><br>

                                {encodedMessage}

                            </p>


                            <hr style=""
                                border:0;
                                border-top:1px solid #30363d;
                                margin:25px 0;"">


                            <p style=""
                                margin:0;
                                color:#8b949e;
                                font-size:12px;
                                line-height:1.6;"">

                                Diese Nachricht wurde über das
                                Kontaktformular von mschott.dev
                                versendet.

                            </p>

                        </td>
                    </tr>

                </table>

            </td>
        </tr>

    </table>

</body>
</html>";

            var plainTextContent =
                $"Neue Kontaktanfrage über mschott.dev\n\n" +
                $"Name: {name}\n" +
                $"E-Mail: {email}\n\n" +
                $"Nachricht:\n{message}";

            // Hotmail
            await SendEmail(
                "matthiasschott@hotmail.de",
                subject,
                htmlContent,
                plainTextContent,
                email,
                name
            );

            // WEB.DE
            await SendEmail(
                "m.m.schott@web.de",
                subject,
                htmlContent,
                plainTextContent,
                email,
                name
            );
        }


        // =========================================================
        // SendGrid Versendelogik
        // =========================================================

        private async Task SendEmail(
            string toEmail,
            string subject,
            string htmlContent,
            string plainTextContent,
            string? replyToEmail = null,
            string? replyToName = null)
        {
            var apiKey =
                _configuration["SendGrid:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new Exception(
                    "SendGrid API Key fehlt in der Konfiguration."
                );
            }

            var client =
                new SendGridClient(apiKey);

            var from =
                new EmailAddress(
                    "info@mschott.dev",
                    "Matthias Schott"
                );

            var to =
                new EmailAddress(toEmail);

            var msg =
                MailHelper.CreateSingleEmail(
                    from,
                    to,
                    subject,
                    plainTextContent,
                    htmlContent
                );


            // Reply-To für Kontaktformular
            if (!string.IsNullOrWhiteSpace(replyToEmail))
            {
                msg.SetReplyTo(
                    new EmailAddress(
                        replyToEmail,
                        replyToName ?? ""
                    )
                );
            }


            var response =
                await client.SendEmailAsync(msg);

            var body =
                await response.Body.ReadAsStringAsync();

            Console.WriteLine("===== SENDGRID =====");
            Console.WriteLine(
                $"Status: {(int)response.StatusCode}"
            );
            Console.WriteLine($"Body: {body}");
            Console.WriteLine("====================");


            if ((int)response.StatusCode < 200 ||
                (int)response.StatusCode >= 300)
            {
                throw new Exception(
                    $"SendGrid Fehler {(int)response.StatusCode}: {body}"
                );
            }
        }
    }
}