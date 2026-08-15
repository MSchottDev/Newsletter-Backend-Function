using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Newsletter_Backend_Function.Data;
using Newsletter_Backend_Function.Services;
using System.Net;
using System.Text.Json;

namespace Newsletter_Backend_Function.Functions
{
    public class SendNewsletter
    {
        private readonly NewsletterDbContext _context;
        private readonly EmailService _emailService;

        public SendNewsletter(
            NewsletterDbContext context,
            EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [Function("SendNewsletter")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "newsletter/send")]
            HttpRequestData req)
        {
            try
            {
                var request = await JsonSerializer.DeserializeAsync<NewsletterRequest>(
                    req.Body,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (request == null ||
                    string.IsNullOrWhiteSpace(request.Title) ||
                    string.IsNullOrWhiteSpace(request.RepoName) ||
                    string.IsNullOrWhiteSpace(request.Description) ||
                    string.IsNullOrWhiteSpace(request.RepoLink))
                {
                    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);

                    await badRequest.WriteAsJsonAsync(new
                    {
                        message = "Newsletter-Daten sind unvollständig."
                    });

                    return badRequest;
                }

                // Nur bestätigte Subscriber laden
                var subscribers = await _context.Subscribers
                    .Where(s => s.IsConfirmed)
                    .ToListAsync();

                if (subscribers.Count == 0)
                {
                    var noSubscribers = req.CreateResponse(HttpStatusCode.OK);

                    await noSubscribers.WriteAsJsonAsync(new
                    {
                        message = "Keine bestätigten Subscriber vorhanden.",
                        sent = 0
                    });

                    return noSubscribers;
                }

                var sent = 0;
                var failed = 0;

                foreach (var subscriber in subscribers)
                {
                    try
                    {
                        await _emailService.SendNewsletterEmail(
                            subscriber.Email,
                            subscriber.UnsubscribeToken,
                            request.Title,
                            request.RepoName,
                            request.Description,
                            request.RepoLink);

                        sent++;
                    }
                    catch (Exception ex)
                    {
                        failed++;

                        Console.WriteLine(
                            $"Newsletter konnte nicht an {subscriber.Email} gesendet werden.");

                        Console.WriteLine(ex.ToString());
                    }
                }

                var response = req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(new
                {
                    message = "Newsletter-Versand abgeschlossen.",
                    totalSubscribers = subscribers.Count,
                    sent = sent,
                    failed = failed
                });

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== NEWSLETTER ERROR =====");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("============================");

                var errorResponse =
                    req.CreateResponse(HttpStatusCode.InternalServerError);

                await errorResponse.WriteAsJsonAsync(new
                {
                    message = "Beim Newsletter-Versand ist ein Fehler aufgetreten.",
                    error = ex.ToString()
                });

                return errorResponse;
            }
        }
    }


    public class NewsletterRequest
    {
        public string Title { get; set; } = string.Empty;

        public string RepoName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string RepoLink { get; set; } = string.Empty;
    }
}