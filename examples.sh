#!/bin/bash
# Example usage scripts for the Watchdog application

echo "Watchdog Application - Usage Examples"
echo "====================================="
echo ""

# Build the application
echo "Building Watchdog application..."
dotnet build --configuration Release

echo ""
echo "Build complete! Watchdog.exe is ready to use."
echo ""
echo "Usage Examples:"
echo "1. Monitor Notepad:"
echo "   Watchdog.exe notepad.exe"
echo ""
echo "2. Monitor an application with arguments:"
echo "   Watchdog.exe C:\MyApp\myapp.exe --verbose --port 8080"
echo ""
echo "3. Monitor with quoted paths:"
echo "   Watchdog.exe \"C:\Program Files\MyApp\app.exe\" --config config.json"
echo ""
echo "Press Ctrl+C at any time to stop the watchdog."
