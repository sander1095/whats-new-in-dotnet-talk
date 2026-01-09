# Real-time Task Progress Monitor - ASP.NET Core 10 SSE Demo

This demo application showcases the new **Server-Sent Events (SSE)** features introduced in ASP.NET Core 10 Preview. It implements a real-time task progress monitoring system where multiple users can track their task executions with automatic reconnection support.

## 🚀 Key Features Demonstrated

### 1. **Server-Sent Events with TypedResults.ServerSentEvents**
- Uses the new `TypedResults.ServerSentEvents()` API introduced in .NET 10 Preview 4
- Streams real-time task progress updates from server to client
- Automatic handling of `text/event-stream` content type and HTTP headers

### 2. **Reconnection with Last-Event-ID Support**
- Implements automatic reconnection handling using the `Last-Event-ID` header
- Server resends missed events when clients reconnect after network interruptions
- Demonstrates robust connection recovery for production scenarios

### 3. **User-Specific Event Filtering**
- Each user receives only their own task updates through parameterized endpoints
- Showcases how to implement multi-tenant SSE streams efficiently
- Supports concurrent users with isolated event streams

### 4. **Advanced SSE Features**
- Custom event IDs for reliable message ordering
- Named event types (`taskUpdate`) for structured client handling
- Heartbeat mechanism to maintain connection health
- Event history management with replay capability

## 🏗️ Architecture Overview

```
┌─────────────────┐    SSE Stream     ┌─────────────────┐
│   Web Client    │◄────────────────-─│   ASP.NET Core  │
│   (Browser)     │  /api/task-       │   Web API       │
│                 │  progress/{user}  │                 │
└─────────────────┘                   └─────────────────┘
        │                                     │
        │ HTTP POST                           │
        │ /api/tasks/{user}/start             │
        └────────────────────────────────────►│
                                              │
                                    ┌─────────▼──────────┐
                                    │ TaskProgressService│
                                    │                    │
                                    │ • Task Execution   │
                                    │ • Event History    │
                                    │ • Progress Stream  │
                                    └────────────────────┘
```

## 🛠️ Technical Implementation

### Backend Components

1. **TaskProgressService**
   - Manages task execution with realistic progress simulation
   - Maintains per-user event history for reconnection scenarios
   - Generates `IAsyncEnumerable<TaskProgressEvent>` streams

2. **UserSessionService**
   - Tracks active user sessions
   - Provides session cleanup and monitoring capabilities

3. **SSE Endpoint Implementation**
   ```csharp
   app.MapGet("/api/task-progress/{userId}", (
       string userId,
       TaskProgressService taskService,
       HttpRequest httpRequest,
       CancellationToken ct) =>
   {
       var lastEventId = httpRequest.Headers.TryGetValue("Last-Event-ID", out var id)
           ? id.ToString() : null;

       var stream = taskService.GetTaskProgressUpdates(userId, lastEventId, ct)
           .Select(progress => new SseItem<TaskProgressEvent>(progress, "taskUpdate")
           {
               EventId = progress.EventId
           });

       return TypedResults.ServerSentEvents(stream);
   });
   ```

### Frontend Implementation

- **Native EventSource API** for SSE consumption
- **Automatic reconnection** with connection state monitoring
- **Real-time UI updates** with progress bars and animations
- **Multi-user simulation** capability for testing concurrent scenarios

## 🎯 Demo Scenarios

### 1. **Basic SSE Streaming**
1. Select a user and connect to the SSE endpoint
2. Start a task to see real-time progress updates
3. Observe the structured event flow in the Event Log

### 2. **Multi-User Concurrent Sessions**
1. Open multiple browser tabs
2. Select different users in each tab
3. Start tasks simultaneously to see isolated user streams

### 3. **Reconnection Testing**
1. Start a long-running task
2. Refresh the page or temporarily disable network
3. Watch automatic reconnection and missed event replay

### 4. **Connection Monitoring**
1. Monitor connection state indicators
2. Observe heartbeat events during idle periods
3. Track Last-Event-ID progression

## 🔧 Getting Started

### Prerequisites
- .NET 10 SDK Preview 7 or later
- A modern web browser with EventSource support

### Running the Demo

1. **Clone and navigate to the project directory**
   ```bash
   cd /path/to/SSE
   ```

2. **Run the application**
   ```bash
   dotnet run
   ```

3. **Open your browser**
   ```
   http://localhost:5000
   ```

4. **Test the features**
   - Select different users
   - Start various tasks
   - Open multiple tabs for concurrent testing
   - Test reconnection by refreshing pages

## 📊 Event Structure

### Task Progress Event
```json
{
  "eventId": "2025-09-02T10:30:45.123Z",
  "taskId": "abc12345",
  "userId": "alice",
  "taskName": "Data Processing",
  "progressPercentage": 60,
  "status": "running",
  "timestamp": "2025-09-02T10:30:45.123Z",
  "message": "Processing data..."
}
```

### SSE Wire Format
```
event: taskUpdate
id: 2025-09-02T10:30:45.123Z
data: {"eventId":"2025-09-02T10:30:45.123Z","taskId":"abc12345"...}

```

## 🆚 SSE vs SignalR Comparison

| Feature | Server-Sent Events | SignalR |
|---------|-------------------|---------|
| **Communication** | Unidirectional (Server → Client) | Bidirectional |
| **Protocol** | HTTP/1.1 with `text/event-stream` | WebSocket + fallbacks |
| **Browser Support** | Native EventSource API | Library required |
| **Reconnection** | Built-in with Last-Event-ID | Built-in with custom logic |
| **Overhead** | Minimal HTTP streaming | WebSocket handshake + framing |
| **Scalability** | Standard HTTP load balancing | Requires sticky sessions or backplane |
| **Use Cases** | Notifications, live feeds, progress tracking | Chat, collaboration, gaming |

## 🎯 Key Benefits Showcased

1. **Simplicity**: SSE requires minimal setup compared to WebSocket solutions
2. **Reliability**: Built-in reconnection with event replay capabilities
3. **Performance**: Lightweight streaming with efficient resource usage
4. **Debugging**: Easy to monitor and debug using standard HTTP tools
5. **Infrastructure**: Works with existing HTTP infrastructure and security

## 🔍 Code Highlights

### ASP.NET Core 10 SSE Features Used

- **`TypedResults.ServerSentEvents()`** - New typed result for SSE responses
- **`SseItem<T>`** - Structured SSE item with event ID and type
- **Built-in cancellation token support** - Automatic connection cleanup
- **Last-Event-ID header handling** - Native reconnection support

### Advanced Patterns Demonstrated

- **Event sourcing** with replay capability
- **Multi-tenant streaming** with user isolation
- **Connection health monitoring** with heartbeats
- **Graceful error handling** and recovery

## 📝 Article Content Suggestions

When writing about this demo for your tech magazine article, consider highlighting:

1. **The Evolution**: How SSE fills the gap between polling and full WebSockets
2. **Real-world Applications**: Task monitoring, live dashboards, notifications
3. **Developer Experience**: Simplicity compared to SignalR for one-way scenarios
4. **Production Readiness**: Built-in features like reconnection and error handling
5. **Performance Characteristics**: Lower overhead and better resource utilization

## 🚀 Future Enhancements

This demo could be extended with:
- Authentication and authorization for user streams
- Redis backplane for multi-server scenarios
- Rate limiting and connection throttling
- Metrics and monitoring integration
- Mobile client implementations

---

*This demo showcases the power and simplicity of Server-Sent Events in ASP.NET Core 10, providing a solid foundation for building real-time web applications with minimal complexity.*
