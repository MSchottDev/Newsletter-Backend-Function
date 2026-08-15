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
    public class AddSubscriber
    {
        private readonly NewsletterDbContext _context;
        private readonly EmailService _emailService;

        public AddSubscriber(
            NewsletterDbContext context,
            EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [Function("AddSubscriber")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "subscribers")]
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

            var existingSubscriber = await _context.Subscribers
                .FirstOrDefaultAsync(s => s.Email == request.Email);

            if (existingSubscriber != null)
            {
                if (existingSubscriber.IsConfirmed)
                {
                    var conflict = req.CreateResponse(HttpStatusCode.Conflict);

                    await conflict.WriteAsJsonAsync(new
                    {
                        message = "Diese Email-Adresse ist bereits für den Newsletter registriert."
                    });

                    return conflict;
                }

                var pending = req.CreateResponse(HttpStatusCode.Conflict);

                await pending.WriteAsJsonAsync(new
                {
                    message = "Diese Email-Adresse wurde bereits registriert. Bitte bestätige die Bestätigungsmail."
                });

                return pending;
            }

            var confirmationToken = Guid.NewGuid().ToString();
            var unsubscribeToken = Guid.NewGuid().ToString();

            var subscriber = new Subscriber
            {
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                IsConfirmed = false,
                ConfirmationToken = confirmationToken,
                UnsubscribeToken = unsubscribeToken,
                TokenCreatedAt = DateTime.UtcNow
            };

            _context.Subscribers.Add(subscriber);

            await _context.SaveChangesAsync();

            try
            {
                await _emailService.SendConfirmationEmail(
                    subscriber.Email,
                    confirmationToken,
                    unsubscribeToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== EMAIL ERROR =====");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("=======================");

                var emailError = req.CreateResponse(HttpStatusCode.InternalServerError);

                await emailError.WriteAsJsonAsync(new
                {
                    message = "Email gespeichert, Senden schlägt fehl",
                    error = ex.ToString()
                });

                return emailError;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);

            await response.WriteAsJsonAsync(new
            {
                message = "Erfolgreich gespeichert. Bitte bestätige deine Email."
            });

            return response;
        }
    }
}
