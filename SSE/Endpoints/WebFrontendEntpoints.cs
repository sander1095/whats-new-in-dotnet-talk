namespace TaskProgressDemo.Endpoints;

public static class WebFrontendEntpoints
{
    public static WebApplication MapWebFrontendEndpoints(this WebApplication app)
    {
        // Serve the main page
        app.MapGet("/", () => Results.Redirect("/taskprogress.html"));

        // Serve the simple demo page
        app.MapGet("/simple", () => Results.Redirect("/randomnumbers.html"));
        
        // Serve the complex demo page
        app.MapGet("/complex", () => Results.Redirect("/taskprogress.html"));

        return app;
    }
}