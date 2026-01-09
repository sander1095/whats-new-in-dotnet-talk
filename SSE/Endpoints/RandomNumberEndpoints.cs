using Microsoft.AspNetCore.Http.HttpResults;
using TaskProgressDemo.RandomNumbers;

namespace TaskProgressDemo.Endpoints;

public static class RandomNumberEndpoints
{
    public static WebApplication MapRandomNumberEndpoints(this WebApplication app)
    {
        app.MapGet("/api/randomnumbers", RandomNumberStream);

        return app;
    }
    
    private static ServerSentEventsResult<int> RandomNumberStream(CancellationToken ct)
    {
        return TypedResults.ServerSentEvents(RandomNumberGenerator.GenerateRandomNumbers(ct));
    }
}