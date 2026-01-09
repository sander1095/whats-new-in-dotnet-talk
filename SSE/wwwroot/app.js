class TaskProgressMonitor {
    constructor() {
        this.currentUser = null;
        this.eventSource = null;
        this.tasks = new Map();
        this.eventCount = 0;
        
        this.initializeElements();
        this.bindEvents();
        this.updateActiveUsers();
        
        // Update active users every 10 seconds
        setInterval(() => this.updateActiveUsers(), 10000);
    }

    initializeElements() {
        this.elements = {
            userSelect: document.getElementById('userSelect'),
            switchUser: document.getElementById('switchUser'),
            taskType: document.getElementById('taskType'),
            startTask: document.getElementById('startTask'),
            connectionStatus: document.getElementById('connectionStatus'),
            connectionText: document.getElementById('connectionText'),
            activeUsers: document.getElementById('activeUsers'),
            sseEndpoint: document.getElementById('sseEndpoint'),
            lastEventId: document.getElementById('lastEventId'),
            readyState: document.getElementById('readyState'),
            noTasks: document.getElementById('noTasks'),
            taskList: document.getElementById('taskList'),
            eventLog: document.getElementById('eventLog'),
            clearLog: document.getElementById('clearLog')
        };
    }

    bindEvents() {
        this.elements.switchUser.addEventListener('click', () => this.switchUser());
        this.elements.startTask.addEventListener('click', () => this.startTask());
        this.elements.clearLog.addEventListener('click', () => this.clearEventLog());
        
        // Auto-connect to first user on page load
        setTimeout(() => this.switchUser(), 500);
    }

    switchUser() {
        const selectedUser = this.elements.userSelect.value;
        
        if (this.eventSource) {
            this.logEvent('SYSTEM', `Disconnecting from user: ${this.currentUser}`);
            this.eventSource.close();
        }

        this.currentUser = selectedUser;
        this.tasks.clear();
        this.updateTaskDisplay();
        this.connectToSSE();
    }

    connectToSSE() {
        const endpoint = `/api/task-progress/${this.currentUser}`;
        this.elements.sseEndpoint.textContent = endpoint;
        
        this.logEvent('SYSTEM', `Connecting to SSE endpoint for user: ${this.currentUser}`);
        
        this.eventSource = new EventSource(endpoint);
        
        this.eventSource.onopen = () => {
            this.updateConnectionStatus('connected', 'Connected');
            this.elements.startTask.disabled = false;
            this.updateReadyState();
            this.logEvent('SSE', 'Connection opened successfully');
        };

        this.eventSource.addEventListener('taskUpdate', (event) => {
            this.handleTaskUpdate(event);
        });

        this.eventSource.onerror = (error) => {
            this.updateConnectionStatus('error', 'Connection Error');
            this.elements.startTask.disabled = true;
            this.updateReadyState();
            this.logEvent('ERROR', `SSE connection error: ${error.type}`);
        };

        this.eventSource.onmessage = (event) => {
            this.elements.lastEventId.textContent = this.eventSource.lastEventId || 'None';
            this.updateReadyState();
        };
    }

    handleTaskUpdate(event) {
        try {
            const data = JSON.parse(event.data);
            this.elements.lastEventId.textContent = event.lastEventId || data.eventId;
            
            // Handle heartbeat separately
            if (data.status === 'heartbeat') {
                this.logEvent('HEARTBEAT', 'Connection heartbeat received');
                return;
            }

            // Check if this is a new task or an update to existing task
            const isNewTask = !this.tasks.has(data.taskId);
            
            // Update or create task
            this.tasks.set(data.taskId, data);
            this.updateTaskDisplay(isNewTask ? data.taskId : null);
            
            // If it's an update to existing task, highlight the progress
            if (!isNewTask) {
                setTimeout(() => {
                    const taskElement = document.querySelector(`[data-task-id="${data.taskId}"]`);
                    if (taskElement) {
                        // Add update highlight
                        taskElement.classList.add('task-updated');
                        
                        // Add progress glow to the progress bar
                        const progressBar = taskElement.querySelector('.progress-bar');
                        if (progressBar) {
                            progressBar.classList.add('progress-pulse');
                        }
                        
                        // Remove animations after completion
                        setTimeout(() => {
                            taskElement.classList.remove('task-updated');
                            if (progressBar) {
                                progressBar.classList.remove('progress-pulse');
                            }
                        }, 800);
                    }
                }, 50);
            }
            
            // Log the event
            const logMessage = `[${data.taskName}] ${data.progressPercentage}% - ${data.message || data.status}`;
            this.logEvent('TASK', logMessage);
            
        } catch (error) {
            this.logEvent('ERROR', `Failed to parse task update: ${error.message}`);
        }
    }

    async startTask() {
        if (!this.currentUser) return;

        const taskOption = this.elements.taskType.value;
        const [taskName, duration] = taskOption.split('|');
        
        try {
            const response = await fetch(`/api/tasks/${this.currentUser}/start`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    taskName: taskName,
                    estimatedDuration: parseInt(duration)
                })
            });

            if (response.ok) {
                const result = await response.json();
                this.logEvent('API', `Started task: ${taskName} (ID: ${result.taskId})`);
            } else {
                this.logEvent('ERROR', `Failed to start task: ${response.statusText}`);
            }
        } catch (error) {
            this.logEvent('ERROR', `Failed to start task: ${error.message}`);
        }
    }

    updateTaskDisplay(newTaskId = null) {
        const taskArray = Array.from(this.tasks.values()).sort((a, b) => 
            new Date(b.timestamp) - new Date(a.timestamp)
        );

        if (taskArray.length === 0) {
            this.elements.noTasks.classList.remove('hidden');
            this.elements.taskList.classList.add('hidden');
            return;
        }

        this.elements.noTasks.classList.add('hidden');
        this.elements.taskList.classList.remove('hidden');

        this.elements.taskList.innerHTML = taskArray.map(task => 
            this.createTaskHTML(task, task.taskId === newTaskId)
        ).join('');
    }

    createTaskHTML(task, isNewTask = false) {
        const statusColors = {
            'running': 'bg-blue-500',
            'completed': 'bg-green-500',
            'cancelled': 'bg-red-500',
            'failed': 'bg-red-500'
        };

        const statusIcons = {
            'running': '⏳',
            'completed': '✅',
            'cancelled': '❌',
            'failed': '💥'
        };

        const progressColor = statusColors[task.status] || 'bg-gray-500';
        const statusIcon = statusIcons[task.status] || '❓';
        const itemClass = isNewTask ? 'task-item' : 'task-item existing';
        
        return `
            <div class="${itemClass} border rounded-lg p-4" data-task-id="${task.taskId}">
                <div class="flex justify-between items-start mb-2">
                    <div>
                        <h4 class="font-semibold text-gray-800">${statusIcon} ${task.taskName}</h4>
                        <p class="text-sm text-gray-600">Task ID: ${task.taskId}</p>
                    </div>
                    <div class="text-right">
                        <span class="text-sm font-medium ${task.status === 'completed' ? 'text-green-600' : task.status === 'running' ? 'text-blue-600' : 'text-red-600'}">
                            ${task.status.toUpperCase()}
                        </span>
                        <p class="text-xs text-gray-500">${new Date(task.timestamp).toLocaleTimeString()}</p>
                    </div>
                </div>
                
                <div class="mb-2">
                    <div class="flex justify-between text-sm mb-1">
                        <span>${task.message || 'Processing...'}</span>
                        <span class="font-medium">${task.progressPercentage}%</span>
                    </div>
                    <div class="w-full bg-gray-200 rounded-full h-2">
                        <div class="progress-bar ${progressColor} h-2 rounded-full" style="width: ${task.progressPercentage}%"></div>
                    </div>
                </div>
            </div>
        `;
    }

    updateConnectionStatus(status, text) {
        const statusEl = this.elements.connectionStatus;
        const textEl = this.elements.connectionText;
        
        statusEl.className = 'connection-indicator w-3 h-3 rounded-full';
        
        switch (status) {
            case 'connected':
                statusEl.classList.add('bg-green-500', 'connected');
                break;
            case 'error':
                statusEl.classList.add('bg-red-500');
                break;
            default:
                statusEl.classList.add('bg-gray-400');
        }
        
        textEl.textContent = text;
    }

    updateReadyState() {
        const states = ['CONNECTING', 'OPEN', 'CLOSED'];
        this.elements.readyState.textContent = states[this.eventSource?.readyState] || 'CLOSED';
    }

    async updateActiveUsers() {
        try {
            const response = await fetch('/api/users/active');
            if (response.ok) {
                const data = await response.json();
                this.elements.activeUsers.textContent = data.activeUsers;
            }
        } catch (error) {
            this.logEvent('ERROR', `Failed to update active users: ${error.message}`);
        }
    }

    logEvent(type, message) {
        this.eventCount++;
        const timestamp = new Date().toLocaleTimeString();
        const logEntry = `[${timestamp}] [${type}] ${message}\n`;
        
        this.elements.eventLog.textContent += logEntry;
        this.elements.eventLog.scrollTop = this.elements.eventLog.scrollHeight;
        
        // Keep only last 100 log entries
        const lines = this.elements.eventLog.textContent.split('\n');
        if (lines.length > 100) {
            this.elements.eventLog.textContent = lines.slice(-100).join('\n');
        }
    }

    clearEventLog() {
        this.elements.eventLog.textContent = '';
        this.eventCount = 0;
        this.logEvent('SYSTEM', 'Event log cleared');
    }
}

// Initialize the application when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new TaskProgressMonitor();
});
