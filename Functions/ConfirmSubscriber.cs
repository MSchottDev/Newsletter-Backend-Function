using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Newsletter_Backend_Function.Data;
using Newsletter_Backend_Function.Services;
using System.Net;

namespace Newsletter_Backend_Function.Functions
{
    public class ConfirmSubscriber
    {
        private readonly NewsletterDbContext _context;
        private readonly EmailService _emailService;

        public ConfirmSubscriber(
            NewsletterDbContext context,
            EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [Function("ConfirmSubscriber")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "subscribers/confirm")]
            HttpRequestData req)
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var token = query["token"];

            if (string.IsNullOrWhiteSpace(token))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);

                await badRequest.WriteAsJsonAsync(new
                {
                    message = "Token fehlt."
                });

                return badRequest;
            }

            var subscriber = await _context.Subscribers
                .FirstOrDefaultAsync(s => s.ConfirmationToken == token);

            if (subscriber == null)
            {
                return CreateRedirectResponse(
                    req,
                    "https://newsletter.mschott.dev/status.html?state=error");
            }

            if (subscriber.IsConfirmed)
            {
                return CreateRedirectResponse(
                    req,
                    "https://newsletter.mschott.dev/status.html?state=already-confirmed");
            }

            subscriber.IsConfirmed = true;

            await _context.SaveChangesAsync();

            await _emailService.SendWelcomeEmail(
                subscriber.Email,
                subscriber.UnsubscribeToken);

            return CreateRedirectResponse(
                req,
                "https://newsletter.mschott.dev/status.html?state=confirmed");
        }

        private static HttpResponseData CreateRedirectResponse(
            HttpRequestData req,
            string location)
        {
            var response = req.CreateResponse(HttpStatusCode.Redirect);
            response.Headers.Add("Location", location);

            return response;
        }
    }
}

