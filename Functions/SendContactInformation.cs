using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newsletter_Backend_Function.Services;

namespace Newsletter_Backend_Function.Functions;

public class SendContactMessage
{
    private readonly EmailService _emailService;

    public SendContactMessage(EmailService emailService)
    {
        _emailService = emailService;
    }

    [Function("SendContactMessage")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "contact")]
        HttpRequestData req)
    {
        try
        {
            var data = await JsonSerializer.DeserializeAsync<ContactRequest>(req.Body);

            if (data == null ||
                string.IsNullOrWhiteSpace(data.Name) ||
                string.IsNullOrWhiteSpace(data.Email) ||
                string.IsNullOrWhiteSpace(data.Message))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);

                await badRequest.WriteAsJsonAsync(new
                {
                    message = "Bitte füllen Sie alle Felder aus."
                });

                return badRequest;
            }

            await _emailService.SendContactEmail(
                data.Name,
                data.Email,
                data.Message
            );

            var response = req.CreateResponse(HttpStatusCode.OK);

            await response.WriteAsJsonAsync(new
            {
                message = "Nachricht erfolgreich gesendet."
            });

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine("===== CONTACT EMAIL ERROR =====");
            Console.WriteLine(ex);
            Console.WriteLine("==============================");

            var error = req.CreateResponse(
                HttpStatusCode.InternalServerError);

            await error.WriteAsJsonAsync(new
            {
                message = "Die Nachricht konnte nicht gesendet werden.",
                error = ex.ToString()
            });

            return error;
        }
    }
}

public class ContactRequest
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Message { get; set; } = "";
}