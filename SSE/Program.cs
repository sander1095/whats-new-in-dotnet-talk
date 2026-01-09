using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Collections.Concurrent;
using TaskProgressDemo;
using TaskProgressDemo.Endpoints;
using TaskProgressDemo.TaskProgress;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel for better shutdown handling
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

// Register our services
builder.Services.AddSingleton<TaskProgressService>();
builder.Services.AddSingleton<UserSessionService>();

// Configure CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Use CORS
app.UseCors("AllowFrontend");

// Serve static files
app.UseStaticFiles();

// SSE endpoint for task progress updates with user-specific filtering
app.MapTaskProgressEndpoints();

// Simple SSE endpoint - Random numbers every second
app.MapRandomNumberEndpoints();

app.MapWebFrontendEndpoints();

// Configure graceful shutdown
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    app.Logger.LogInformation("Application is shutting down gracefully...");
});

// Handle Ctrl+C gracefully
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    app.Logger.LogInformation("Received shutdown signal, stopping application...");
    lifetime.StopApplication();
};

app.Run();






