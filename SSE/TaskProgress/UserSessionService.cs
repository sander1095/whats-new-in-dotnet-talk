using System.Collections.Concurrent;

namespace TaskProgressDemo.TaskProgress;

public class UserSessionService : IDisposable
{
    private readonly ConcurrentDictionary<string, DateTime> _activeSessions = new();
    private readonly Timer _cleanupTimer;

    public UserSessionService()
    {
        // Clean up inactive sessions every minute
        _cleanupTimer = new Timer(CleanupInactiveSessions, null, 
            TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public void RegisterUser(string userId)
    {
        _activeSessions.AddOrUpdate(userId, DateTime.UtcNow, (_, _) => DateTime.UtcNow);
    }

    public int GetActiveUsersCount()
    {
        CleanupInactiveSessions(null);
        return _activeSessions.Count;
    }

    private void CleanupInactiveSessions(object? state)
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-5); // 5 minutes timeout
        
        var inactiveUsers = _activeSessions
            .Where(kvp => kvp.Value < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var userId in inactiveUsers)
        {
            _activeSessions.TryRemove(userId, out _);
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}
