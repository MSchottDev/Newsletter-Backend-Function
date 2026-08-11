using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newsletter_Backend_Function.Data;
using Newsletter_Backend_Function.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.ConfigureFunctionsWebApplication();

// CORS konfigurieren
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("https://newsletter.mschott.dev")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Datenbank
builder.Services.AddDbContext<NewsletterDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// EmailService
builder.Services.AddScoped<EmailService>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();