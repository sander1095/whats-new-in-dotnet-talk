using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace TaskProgressDemo.TaskProgress;

public class TaskProgressService : IDisposable
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<TaskProgressEvent>> _userTaskHistory = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _runningTasks = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<IObserver<TaskProgressEvent>>> _userObservers = new();
    private readonly SemaphoreSlim _historySemaphore = new(1, 1);
    private readonly ILogger<TaskProgressService> _logger;

    public TaskProgressService(ILogger<TaskProgressService> logger)
    {
        _logger = logger;
    }

    public string StartTask(string userId, string taskName, int estimatedDurationSeconds)
    {
        var taskId = Guid.NewGuid().ToString("N")[..8];
        var cts = new CancellationTokenSource();
        
        _runningTasks[taskId] = cts;
        
        // Start the task execution in background
        _ = Task.Run(async () => await ExecuteTaskAsync(userId, taskId, taskName, estimatedDurationSeconds, cts.Token));
        
        _logger.LogInformation("Started task {TaskId} for user {UserId}: {TaskName}", taskId, userId, taskName);
        return taskId;
    }

    private async Task ExecuteTaskAsync(string userId, string taskId, string taskName, int durationSeconds, CancellationToken cancellationToken)
    {
        try
        {
            var steps = new[]
            {
                "Initializing...",
                "Processing data...",
                "Validating results...",
                "Generating report...",
                "Finalizing..."
            };

            var stepDuration = durationSeconds * 1000 / steps.Length;

            for (int i = 0; i < steps.Length && !cancellationToken.IsCancellationRequested; i++)
            {
                var progress = (int)((i + 1) * 100.0 / steps.Length);
                var status = i == steps.Length - 1 ? "completed" : "running";

                var progressEvent = new TaskProgressEvent(
                    EventId: DateTime.UtcNow.ToString("o"),
                    TaskId: taskId,
                    UserId: userId,
                    TaskName: taskName,
                    ProgressPercentage: progress,
                    Status: status,
                    Timestamp: DateTime.UtcNow,
                    Message: steps[i]
                );

                await AddEventToHistoryAsync(userId, progressEvent);
                NotifyObservers(userId, progressEvent);
                await Task.Delay(stepDuration, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            var cancelledEvent = new TaskProgressEvent(
                EventId: DateTime.UtcNow.ToString("o"),
                TaskId: taskId,
                UserId: userId,
                TaskName: taskName,
                ProgressPercentage: 0,
                Status: "cancelled",
                Timestamp: DateTime.UtcNow,
                Message: "Task was cancelled"
            );
            
            await AddEventToHistoryAsync(userId, cancelledEvent);
            NotifyObservers(userId, cancelledEvent);
        }
        finally
        {
            _runningTasks.TryRemove(taskId, out _);
        }
    }

    private async Task AddEventToHistoryAsync(string userId, TaskProgressEvent progressEvent)
    {
        await _historySemaphore.WaitAsync();
        try
        {
            var userQueue = _userTaskHistory.GetOrAdd(userId, _ => new ConcurrentQueue<TaskProgressEvent>());
            userQueue.Enqueue(progressEvent);
            
            // Keep only last 50 events per user
            while (userQueue.Count > 50)
            {
                userQueue.TryDequeue(out _);
            }
        }
        finally
        {
            _historySemaphore.Release();
        }
    }

    private void NotifyObservers(string userId, TaskProgressEvent progressEvent)
    {
        if (_userObservers.TryGetValue(userId, out var observersBag))
        {
            var observers = observersBag.ToArray(); // Create a snapshot to avoid collection modification issues
            foreach (var observer in observers)
            {
                try
                {
                    observer.OnNext(progressEvent);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to notify observer for user {UserId}", userId);
                    // Note: ConcurrentBag doesn't have efficient removal, so we'll let cleanup happen naturally
                }
            }
        }
    }

    public async IAsyncEnumerable<TaskProgressEvent> GetTaskProgressUpdates(
        string userId,
        string? lastEventId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Send missed events if reconnecting
        if (!string.IsNullOrEmpty(lastEventId) && _userTaskHistory.TryGetValue(userId, out var userQueue))
        {
            var allEvents = userQueue.ToArray();
            var lastEventIndex = Array.FindIndex(allEvents, e => e.EventId == lastEventId);
            
            if (lastEventIndex >= 0 && lastEventIndex < allEvents.Length - 1)
            {
                // Send events after the last received event
                for (int i = lastEventIndex + 1; i < allEvents.Length; i++)
                {
                    yield return allEvents[i];
                }
            }
        }

        // Send initial heartbeat
        yield return new TaskProgressEvent(
            EventId: DateTime.UtcNow.ToString("o"),
            TaskId: "heartbeat",
            UserId: userId,
            TaskName: "System",
            ProgressPercentage: 0,
            Status: "heartbeat",
            Timestamp: DateTime.UtcNow,
            Message: "Connection established"
        );

        // Use TaskCompletionSource for efficient event waiting
        var eventQueue = new Queue<TaskProgressEvent>();
        var eventSignal = new TaskCompletionSource<bool>();
        var lastHeartbeat = DateTime.UtcNow;

        // Create observer for this connection
        var observer = new TaskProgressObserver(
            onNext: evt =>
            {
                lock (eventQueue)
                {
                    eventQueue.Enqueue(evt);
                    if (!eventSignal.Task.IsCompleted)
                    {
                        eventSignal.SetResult(true);
                    }
                }
            },
            onError: ex => eventSignal.SetException(ex),
            onCompleted: () => eventSignal.SetResult(false)
        );

        // Register observer
        var observersBag = _userObservers.GetOrAdd(userId, _ => new ConcurrentBag<IObserver<TaskProgressEvent>>());
        observersBag.Add(observer);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Wait for events or timeout for heartbeat
                var delay = Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                var completed = await Task.WhenAny(eventSignal.Task, delay);

                if (completed == eventSignal.Task && eventSignal.Task.Result)
                {
                    // Process all queued events
                    lock (eventQueue)
                    {
                        while (eventQueue.Count > 0)
                        {
                            yield return eventQueue.Dequeue();
                        }
                        eventSignal = new TaskCompletionSource<bool>(); // Reset for next batch
                    }
                    lastHeartbeat = DateTime.UtcNow;
                }
                else if (DateTime.UtcNow - lastHeartbeat > TimeSpan.FromSeconds(30))
                {
                    // Send heartbeat
                    yield return new TaskProgressEvent(
                        EventId: DateTime.UtcNow.ToString("o"),
                        TaskId: "heartbeat",
                        UserId: userId,
                        TaskName: "System",
                        ProgressPercentage: 0,
                        Status: "heartbeat",
                        Timestamp: DateTime.UtcNow,
                        Message: "Connection alive"
                    );
                    lastHeartbeat = DateTime.UtcNow;
                }
            }
        }
        finally
        {
            // Clean up observer - Note: ConcurrentBag doesn't have efficient removal
            // In a production scenario, you might want to use a different concurrent collection
            // or implement a more sophisticated cleanup mechanism
        }
    }

    public void Dispose()
    {
        _historySemaphore?.Dispose();
        
        // Cancel all running tasks
        foreach (var cts in _runningTasks.Values)
        {
            cts?.Cancel();
            cts?.Dispose();
        }
        _runningTasks.Clear();
    }

    internal class TaskProgressObserver : IObserver<TaskProgressEvent>
    {
        private readonly Action<TaskProgressEvent> _onNext;
        private readonly Action<Exception> _onError;
        private readonly Action _onCompleted;

        public TaskProgressObserver(Action<TaskProgressEvent> onNext, Action<Exception> onError, Action onCompleted)
        {
            _onNext = onNext;
            _onError = onError;
            _onCompleted = onCompleted;
        }

        public void OnNext(TaskProgressEvent value) => _onNext(value);
        public void OnError(Exception error) => _onError(error);
        public void OnCompleted() => _onCompleted();
    }
}