# Watchdog Application

A robust C# console application that monitors an executable and automatically restarts it if it crashes or exits unexpectedly. Supports single or multiple applications via XML configuration.

## Features

✅ **Process Monitoring** - Continuously monitors the watched application  
✅ **Auto-Restart** - Automatically restarts the application on crash/exit  
✅ **XML Configuration** - Manage multiple applications via config files  
✅ **Application Selection Menu** - Choose which app to monitor from menu  
✅ **Graceful Shutdown** - Cleanly stops both watchdog and monitored app (Ctrl+C)  
✅ **Process ID Tracking** - Displays PID of managed processes  
✅ **Restart Counter** - Tracks total number of restarts  
✅ **Timestamped Logging** - Detailed logs with crash/restart times  
✅ **Log Rotation** - Automatic rotation at 10MB or daily (whichever comes first)  
✅ **Argument Support** - Passes arguments to the monitored executable  
✅ **Crash Prevention** - 2-second delay between restarts to prevent loops  
✅ **Backward Compatible** - Still works with direct command-line usage  

## Requirements

- .NET 6.0 or later
- Windows OS (uses Windows-specific Process APIs)

## Building

```bash
dotnet build
```

## Usage

### Basic Syntax
```
Watchdog.exe <executable_path> [arguments]
Watchdog.exe <config_file.xml>
Watchdog.exe
```

### Examples

**Example 1: Simple executable without arguments**
```bash
Watchdog.exe C:\MyApp\myapp.exe
```

**Example 2: Executable with arguments**
```bash
Watchdog.exe C:\MyApp\myapp.exe --verbose --port 8080
```

**Example 3: Using XML configuration (single app)**
```bash
Watchdog.exe config.xml
```

**Example 4: Automatic XML detection (config.xml in current directory)**
```bash
Watchdog.exe
```

**Example 5: Custom XML file**
```bash
Watchdog.exe myapps.xml
```

## XML Configuration

Store multiple applications in an XML file for centralized management:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Applications>
  <Application>
    <Name>Notepad</Name>
    <Path>notepad.exe</Path>
    <Arguments></Arguments>
  </Application>
  
  <Application>
    <Name>Web Server</Name>
    <Path>C:\MyApp\server.exe</Path>
    <Arguments>--port 8080 --env production</Arguments>
  </Application>
</Applications>
```

### Features:
- **Multiple Applications**: Store multiple applications in one file
- **Selection Menu**: Choose which app to monitor when multiple apps exist
- **Flexible Arguments**: Each app can have custom arguments
- **Automatic Loading**: Place `config.xml` in current directory and run `Watchdog.exe`
- **Custom Config Files**: Use any XML file name with `Watchdog.exe myconfig.xml`

See [XML_CONFIG_GUIDE.md](XML_CONFIG_GUIDE.md) for detailed configuration documentation.

## Console Output

The watchdog displays colored, timestamped messages:

- 🟢 **Green** (✅) - Application started successfully
- 🟡 **Yellow** (⚠️) - Application exited, restart in progress
- 🔴 **Red** (❌) - Errors and failures
- 🔵 **Cyan** (🚀) - Restart information

### Sample Output
```
╔════════════════════════════════════════════════╗
║         Watchdog Application Started           ║
╚════════════════════════════════════════════════╝
📁 Executable: notepad.exe
📋 Application: notepad
⚙️  Arguments: (none)
⏰ Started at: 2026-02-22 10:30:45
📝 Log file: watchdog-notepad-2026-02-22.log
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Press Ctrl+C to stop the watchdog.

[10:30:45] ✅ Application started (PID: 5432)
[10:31:12] ⚠️  CRASH DETECTED
[10:31:12] 🔴 Application 'notepad' crashed/exited
[10:31:12] 📊 Exit Code: 1 | PID: 5432
[10:31:12] 🔄 Restarting application (Attempt #1)...
[10:31:14] 🚀 Restarting 'notepad' now...
[10:31:14] ✅ Application started (PID: 5768)
```

## How It Works

1. **Startup**: Watchdog validates the executable path and starts the application with provided arguments
2. **Monitoring**: Continuously monitors the process using `WaitForExit()`
3. **Detection**: When the process exits, the exit code is logged
4. **Restart**: If watchdog is still running, it waits 2 seconds and restarts the application
5. **Graceful Shutdown**: Pressing Ctrl+C terminates the watchdog and kills the monitored process

## Log Rotation

The watchdog automatically rotates log files to prevent unbounded growth:

### Rotation Triggers
- **Size-based**: Log file rotates when it reaches **10 MB**
- **Time-based**: Log file rotates **daily** at the date change
- **Whichever comes first**: Rotation happens on whichever condition is met first

### Log File Naming

**Current log file:**
```
watchdog-{application-name}-{YYYY-MM-DD}.log
```

**Rotated/archived logs:**
```
watchdog-{application-name}-{YYYY-MM-DD}.1.log      (daily rotation)
watchdog-{application-name}-{YYYY-MM-DD-HHmmss}.1.log (size rotation)
watchdog-{application-name}-{YYYY-MM-DD-HHmmss}.2.log (size rotation #2)
```

### Example
```
2026-02-22
├── watchdog-notepad-2026-02-22.log          (current, 2.5 MB)
├── watchdog-notepad-2026-02-21.1.log        (rotated daily)
├── watchdog-notepad-2026-02-21.2.log        (rotated daily)
└── watchdog-notepad-2026-02-20.log          (from previous days)

2026-02-23
├── watchdog-notepad-2026-02-23.log          (new day, starts fresh)
└── watchdog-notepad-2026-02-22-235959.1.log (size-rotated, 10+ MB)
```

### Console Notification
When a log rotation occurs, the watchdog displays:
```
[HH:mm:ss] 📦 Log rotated (daily) -> watchdog-notepad-2026-02-21.1.log
[HH:mm:ss] 📦 Log rotated (size) -> watchdog-notepad-2026-02-22-153045.1.log
```

## Error Handling

- Invalid executable paths are caught and reported
- Process errors during restart are handled gracefully
- Thread-safe operations using lock objects
- Proper resource disposal to prevent handle leaks
- Log rotation failures are logged but don't stop the watchdog

## Technical Details

### Thread Safety
The application uses a lock object to ensure thread-safe access to the process object when:
- Starting a new process
- Checking process state
- Terminating the process

### Restart Delay
A 2-second delay is implemented between restart attempts to:
- Prevent rapid restart loops from consuming resources
- Allow system to clean up process resources
- Give time for port/resource cleanup if applicable

### Exit Codes
The watchdog logs the exit code of the terminated process, which can help diagnose why it crashed:
- Exit code 0: Normal successful exit
- Exit code non-zero: Indicates an error condition

### XML Configuration Parsing
- Uses System.Xml.Linq for XML parsing
- Supports UTF-8 encoding
- Validates required elements (Name, Path)
- Handles missing or empty Arguments gracefully
- Shows user-friendly error messages for invalid XML

### Logging
- Separate log file for each application: `watchdog-{app-name}-{date}.log`
- Timestamped entries for all events
- Includes application name, PID, exit codes, and restart information
- Auto-flush to ensure data is written immediately

## Stopping the Watchdog

Press **Ctrl+C** in the console to gracefully stop the watchdog:
1. Receives the interrupt signal
2. Terminates the monitored application
3. Displays final statistics
4. Exits cleanly

## Use Cases

- **Development**: Keep test applications running during development
- **Services**: Automatically restart background services on failure
- **Production**: Maintain availability of critical applications
- **Testing**: Repeatedly run applications that may crash
- **CI/CD Pipelines**: Monitor automated processes

## Notes

- The watchdog does not capture the output of the monitored application (processes run in their own window)
- For output capture, modify `UseShellExecute` and `RedirectStandardOutput` properties
- The application is designed for Windows; minor modifications needed for Linux/macOS

---

**Version**: 1.0  
**Created**: 2026-02-22
#   A M S M 
 
 #   A M S M  
 #   A M S M  
 