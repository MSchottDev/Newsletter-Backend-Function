using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Newsletter_Backend_Function.Data;
using System.Net;

namespace Newsletter_Backend_Function.Functions
{
    public class UnsubscribeSubscriber
    {
        private readonly NewsletterDbContext _context;

        public UnsubscribeSubscriber(
            NewsletterDbContext context)
        {
            _context = context;
        }

        [Function("UnsubscribeSubscriber")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "subscribers/unsubscribe")]
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
                .FirstOrDefaultAsync(s => s.UnsubscribeToken == token);

            if (subscriber == null)
            {
                return CreateRedirectResponse(
                    req,
                    "https://newsletter.mschott.dev/status.html?state=error");
            }

            _context.Subscribers.Remove(subscriber);

            await _context.SaveChangesAsync();

            return CreateRedirectResponse(
                req,
                "https://newsletter.mschott.dev/status.html?state=unsubscribed");
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

