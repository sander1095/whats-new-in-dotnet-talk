// Program.cs (.NET 10 Preview)

using Scalar.AspNetCore;
using Validation.Endpoints;

namespace Validation;

// --- Program ---
public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Minimal APIs Problemdetails
        //builder.Services.AddProblemDetails();
        // .NET 10: native OpenAPI-Services registrieren
        builder.Services.AddOpenApi();

        // ASP.NET Core 10 minimal validation
        builder.Services.AddValidation();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();                           // /openapi/v1.json
            app.MapOpenApi("/openapi/v1.yaml");  // /openapi/v1.yaml
            app.MapScalarApiReference();                // /scalar/v1
        }

        app.MapOrderEndpoints();

        app.Run();
    }
}
