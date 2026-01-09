using TaskProgressDemo.TaskProgress;

namespace TaskProgressDemo.Endpoints;

public static class TaskProgressEndpoints
{
    public static WebApplication MapTaskProgressEndpoints(this WebApplication app)
    {
        app.MapGet("/api/task-progress/{userId}", (
            string userId,
            TaskProgressService taskService,
            UserSessionService userService,
            HttpRequest httpRequest,
            HttpResponse httpResponse,
            CancellationToken ct) =>
        {
            try
            {
                // Handle reconnection with Last-Event-ID
                var lastEventId = httpRequest.Headers.TryGetValue("Last-Event-ID", out var id)
                    ? id.ToString()
                    : null;

                if (!string.IsNullOrEmpty(lastEventId))
                {
                    app.Logger.LogInformation("User {UserId} reconnected, last event ID: {LastEventId}", userId,
                        lastEventId);
                }
                else
                {
                    app.Logger.LogInformation("User {UserId} connected to SSE stream", userId);
                }

                // Register user session
                userService.RegisterUser(userId);

                // Create the SSE stream with user-specific filtering
                var stream = taskService.GetTaskProgressUpdates(userId, lastEventId, ct);

                return TypedResults.ServerSentEvents(stream, eventType: "taskUpdate");
            }
            catch (Exception ex)
            {
                app.Logger.LogError(ex, "Error in SSE endpoint for user {UserId}", userId);
                return Results.StatusCode(500);
            }
        });

        // API to start a new task for a specific user
        app.MapPost("/api/tasks/{userId}/start", (
            string userId,
            TaskStartRequest request,
            TaskProgressService taskService) =>
        {
            var taskId = taskService.StartTask(userId, request.TaskName, request.EstimatedDuration);
            return Results.Ok(new { TaskId = taskId, Message = "Task started successfully" });
        });

        // API to get active users count
        app.MapGet("/api/users/active",
            (UserSessionService userService) =>
            {
                return Results.Ok(new { ActiveUsers = userService.GetActiveUsersCount() });
            });

        return app;
    }
}