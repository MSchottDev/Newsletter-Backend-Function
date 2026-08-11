using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Newsletter_Backend_Function.Data;
using Newsletter_Backend_Function.Models;
using Newsletter_Backend_Function.Services;
using System.Net;
using System.Text.Json;

namespace Newsletter_Backend_Function.Functions
{
    public class ResendConfirmation
    {
        private readonly NewsletterDbContext _context;
        private readonly EmailService _emailService;

        public ResendConfirmation(
            NewsletterDbContext context,
            EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [Function("ResendConfirmation")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "subscribers/resend-confirmation")]
            HttpRequestData req)
        {
            var request = await JsonSerializer.DeserializeAsync<CreateSubscriberRequest>(
                req.Body,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (request == null || string.IsNullOrWhiteSpace(request.Email))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);

                await badRequest.WriteAsJsonAsync(new
                {
                    message = "Bitte eine Email eingeben."
                });

                return badRequest;
            }

            var subscriber = await _context.Subscribers
                .FirstOrDefaultAsync(s => s.Email == request.Email);

            if (subscriber == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);

                await notFound.WriteAsJsonAsync(new
                {
                    message = "Email-Adresse nicht gefunden."
                });

                return notFound;
            }

            if (subscriber.IsConfirmed)
            {
                var conflict = req.CreateResponse(HttpStatusCode.Conflict);

                await conflict.WriteAsJsonAsync(new
                {
                    message = "E-Mail bereits bestätigt."
                });

                return conflict;
            }

            var newToken = Guid.NewGuid().ToString();

            subscriber.ConfirmationToken = newToken;
            subscriber.TokenCreatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _emailService.SendConfirmationEmail(
                subscriber.Email,
                newToken,
                subscriber.UnsubscribeToken);

            var response = req.CreateResponse(HttpStatusCode.OK);

            await response.WriteAsJsonAsync(new
            {
                message = "Neue Bestätigungsmail versendet."
            });

            return response;
        }
    }
}

