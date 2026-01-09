namespace TaskProgressDemo.TaskProgress;

public record TaskProgressEvent(
    string EventId,
    string TaskId,
    string UserId,
    string TaskName,
    int ProgressPercentage,
    string Status,
    DateTime Timestamp,
    string? Message = null);