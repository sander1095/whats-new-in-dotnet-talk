#!/bin/bash

# Kill any existing process on port 5000
echo "Checking for existing processes on port 5000..."
PID=$(lsof -ti:5000)
if [ ! -z "$PID" ]; then
    echo "Killing process $PID on port 5000"
    kill -9 $PID
    sleep 1
fi

# Wait a moment for port to be released
echo "Waiting for port to be released..."
sleep 2

# Start the application
echo "Starting ASP.NET Core application..."
dotnet run --project SSEDemo.csproj
