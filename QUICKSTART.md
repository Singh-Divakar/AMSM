# Watchdog Application - Quick Start Guide

## Building the Application

### Option 1: Using .NET CLI
```powershell
cd f:\AMSM
dotnet build
```

### Option 2: Release Build (Optimized)
```powershell
cd f:\AMSM
dotnet build --configuration Release
```

## Running the Watchdog

### Compiled Executable Location
The executable will be found at:
- Debug: `f:\AMSM\bin\Debug\net6.0\Watchdog.exe`
- Release: `f:\AMSM\bin\Release\net6.0\Watchdog.exe`

### Basic Usage

**Syntax:**
```
Watchdog.exe <path_to_executable> [arguments]
```

### Practical Examples

#### 1. Monitor Notepad
```powershell
f:\AMSM\bin\Debug\net6.0\Watchdog.exe notepad.exe
```
Try this: Close notepad and watch it restart automatically!

#### 2. Monitor with Arguments
```powershell
f:\AMSM\bin\Debug\net6.0\Watchdog.exe C:\Windows\System32\calc.exe
```

#### 3. Monitor a Custom Application
```powershell
f:\AMSM\bin\Debug\net6.0\Watchdog.exe "C:\MyApp\myapp.exe" --verbose --port 8080
```

#### 4. Add to Your PATH (Optional)
```powershell
# Copy the executable to a folder in your PATH or create a shortcut
Copy-Item f:\AMSM\bin\Debug\net6.0\Watchdog.exe "C:\Windows\System32\Watchdog.exe"

# Then use it directly:
Watchdog.exe notepad.exe
```

## Testing the Watchdog

### Test Scenario 1: Basic Restart
1. Run: `Watchdog.exe notepad.exe`
2. Close the notepad window
3. Observe: Notepad automatically restarts
4. Stop watchdog: Press Ctrl+C

### Test Scenario 2: With Console Application
Create a test C# console app (test-app.exe) that exits after 5 seconds, then:
```powershell
Watchdog.exe C:\path\to\test-app.exe
```
Watch it restart automatically every 7 seconds (5s + 2s delay)

### Test Scenario 3: Graceful Shutdown
1. Run: `Watchdog.exe notepad.exe`
2. View the status and PID
3. Press Ctrl+C to gracefully shut down
4. Observe the final statistics

## Key Features Demonstrated

✅ Application starts automatically  
✅ PID (Process ID) is displayed  
✅ On exit/crash, watchdog detects it within seconds  
✅ Automatic 2-second delay prevents restart loops  
✅ Timestamps show exactly when events occur  
✅ Restart counter tracks total attempts  
✅ Color-coded messaging for easy reading  
✅ Ctrl+C gracefully stops everything  

## Troubleshooting

### Error: "Failed to start application"
- Verify the full path is correct
- Check if the executable exists
- Ensure you have permission to execute the file
- Try with quotes for paths with spaces: `"C:\Program Files\App\app.exe"`

### Error: "Specify which project or solution file to use"
- Use the full project name: `dotnet build Watchdog.csproj`
- Or build from the directory: `cd f:\AMSM && dotnet build`

### Application won't start in Release mode
```powershell
dotnet publish --configuration Release
# Then run from: f:\AMSM\bin\Release\net6.0\publish\Watchdog.exe
```

## Customization Options

You can modify the source code (f:\AMSM\Program.cs) to:
- Change the restart delay (currently 2000ms)
- Capture stdout/stderr output
- Implement logging to file
- Add configuration file support
- Set resource limits
- Add health checks

## Command Line Examples

```powershell
# Simple example
Watchdog.exe C:\MyApp\app.exe

# With multiple arguments
Watchdog.exe C:\MyApp\app.exe --config config.json --env production

# With quoted paths
Watchdog.exe "C:\Program Files\MyApp\myapp.exe" "arg with spaces"

# Windows service or scheduled task
Watchdog.exe C:\path\to\service.exe
```

---

**Build Status**: ✅ Successfully compiled (net6.0)  
**Ready to use!**
