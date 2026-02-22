# Log Rotation - Complete Guide

## Overview

The Watchdog application includes automatic log rotation to prevent log files from consuming excessive disk space. Logs are rotated based on **two criteria**:

1. **Size-based**: When log file reaches **10 MB**
2. **Time-based**: **Daily** at midnight (date change)

**Rotation happens on whichever condition is met first.**

## Rotation Triggers

### Size-Based Rotation
- ✅ Log file is checked before each write
- ✅ Rotation triggered when file size ≥ 10 MB
- ✅ Allows multiple rotations per day
- ✅ Useful for high-traffic applications

### Time-Based Rotation
- ✅ Log file is checked before each write
- ✅ Rotation triggered when date changes (midnight)
- ✅ One rotation per day (at most)
- ✅ Useful for daily log management

### Combined Approach
The watchdog checks **both conditions** before every log write and rotates on whichever comes first:

```
If Size >= 10 MB  OR  Date changed
   └─► Rotate log file
```

## Log File Naming

### Current Log File (Active)
The current log is always named with the current date:

```
watchdog-{application-name}-{YYYY-MM-DD}.log
```

### Rotated/Archived Log Files
When a log is rotated, it's renamed with a rotation number and timestamp:

**Daily Rotation (date changed):**
```
watchdog-{application-name}-{YYYY-MM-DD}.1.log
watchdog-{application-name}-{YYYY-MM-DD}.2.log
watchdog-{application-name}-{YYYY-MM-DD}.3.log
```

**Size Rotation (10 MB reached):**
```
watchdog-{application-name}-{YYYY-MM-DD-HHmmss}.1.log
watchdog-{application-name}-{YYYY-MM-DD-HHmmss}.2.log
watchdog-{application-name}-{YYYY-MM-DD-HHmmss}.3.log
```

## Rotation Examples

### Example 1: Multiple Rotations in One Day

**Scenario**: Application generates lots of logs, exceeding 10 MB multiple times.

```
Files on same day:
├── watchdog-api-2026-02-22.log              (current, 3.2 MB)
├── watchdog-api-2026-02-22-123045.1.log     (rotated, 10 MB)
├── watchdog-api-2026-02-22-145320.2.log     (rotated, 10 MB)
├── watchdog-api-2026-02-22-163842.3.log     (rotated, 10 MB)
└── watchdog-api-2026-02-21.1.log            (previous day)
```

### Example 2: Daily Rotation

**Scenario**: Application starts on day 1, runs through day 2, and into day 3.

```
Day 1 (2026-02-20):
└── watchdog-notepad-2026-02-20.log          (2.5 MB, ran all day)

Day 2 (2026-02-21):
├── watchdog-notepad-2026-02-21.log          (current on day 2)
├── watchdog-notepad-2026-02-20.log          (archived from day 1)

Day 3 (2026-02-22):
├── watchdog-notepad-2026-02-22.log          (current on day 3)
├── watchdog-notepad-2026-02-21.1.log        (archived from day 2)
├── watchdog-notepad-2026-02-20.log          (archived from day 1)
```

### Example 3: Mixed Size and Daily Rotation

**Scenario**: Application with mixed behavior - sometimes size limit, sometimes daily.

```
Timeline:
2026-02-20
├── watchdog-app-2026-02-20.log              (ran all day, 4 MB)

2026-02-21
├── watchdog-app-2026-02-21.log              (current)
├── watchdog-app-2026-02-21-085634.1.log     (size rotation, 10 MB)
├── watchdog-app-2026-02-21-142211.2.log     (size rotation, 10 MB)
├── watchdog-app-2026-02-21-195448.3.log     (size rotation, 10 MB, 1.2 MB remaining)
└── watchdog-app-2026-02-20.log              (daily rotation from day 1)

2026-02-22 (midnight)
├── watchdog-app-2026-02-22.log              (new day, fresh start)
├── watchdog-app-2026-02-21.4.log            (daily rotation from day 2)
└── watchdog-app-2026-02-21.3.log            (from day 2)
```

## Console Output

When log rotation occurs, you'll see a notification in the console:

### Daily Rotation
```
[HH:mm:ss] 📦 Log rotated (daily) -> watchdog-notepad-2026-02-21.1.log
```

### Size Rotation
```
[HH:mm:ss] 📦 Log rotated (size) -> watchdog-notepad-2026-02-22-143025.1.log
```

### Multiple Rotations
```
[10:15:30] 📦 Log rotated (size) -> watchdog-api-2026-02-22-101530.1.log
[14:45:15] 📦 Log rotated (size) -> watchdog-api-2026-02-22-144515.2.log
[23:59:59] 📦 Log rotated (daily) -> watchdog-api-2026-02-22.3.log
[00:00:01] 📦 Log rotated (daily) -> watchdog-api-2026-02-23.log
```

## Log File Content

Each log file contains all events for that rotation period:

```
═══════════════════════════════════════════════════
Watchdog Started: 2026-02-22 10:15:30
Application: notepad
Executable: notepad.exe
Arguments: (none)
═══════════════════════════════════════════════════

[START] 2026-02-22 10:15:31
Application: notepad
PID: 5432

[CRASH] 2026-02-22 10:35:12
Application: notepad
PID: 5432
Exit Code: 0
Restart Attempt: #1

[RESTART] 2026-02-22 10:35:14
Time between crash and restart: 2.0 seconds

[START] 2026-02-22 10:35:14
Application: notepad
PID: 5768

...additional events...

Log rotated due to size limit exceeding 10 MB
```

## Disk Space Management

### Typical Log Sizes

- **Small applications** (minimal output): 0.1 - 1 MB per day
- **Medium applications** (moderate activity): 1 - 5 MB per day
- **High-traffic applications** (busy processes): 5 - 50+ MB per day

### Retention Strategy

Since logs are rotated at 10 MB or daily, you should:

1. **Monitor disk usage**: Check `watchdog-*.log` files periodically
2. **Archive old logs**: Move archived logs to backup storage if needed
3. **Delete old logs**: Remove logs older than your retention period

### Example Cleanup Script

```powershell
# Delete logs older than 30 days
Get-ChildItem "watchdog-*.log" | Where-Object {
    $_.LastWriteTime -lt (Get-Date).AddDays(-30)
} | Remove-Item

# Archive logs from last month to backup folder
Get-ChildItem "watchdog-*.log" | Where-Object {
    $_.LastWriteTime -lt (Get-Date).AddDays(-7) -and
    $_.LastWriteTime -gt (Get-Date).AddDays(-30)
} | Move-Item -Destination "C:\Backup\Logs\"
```

## Rotation Algorithm

The watchdog uses this algorithm for log rotation:

```
Before each log write:
├─ CheckAndRotateLogFile()
│  ├─ Is today's date different from last log date?
│  │  └─ YES: Call RotateLogFile("daily")
│  │
│  └─ Does current log file size >= 10 MB?
│     └─ YES: Call RotateLogFile("size")
│
└─ Write log message to current file
```

### Rotation Process

```
1. Close current log file (flush & dispose)
2. Archive current file with timestamp/rotation number
3. Display rotation notification to console
4. Create new log file with current date
5. Log index reset for new day (starts at .1)
```

## Best Practices

### 1. Regular Monitoring
```powershell
# Check current log size
Get-Item watchdog-notepad-*.log | 
    Select-Object Name, @{N="Size(MB)";E={[math]::Round($_.Length/1MB,2)}}
```

### 2. Log Analysis
```powershell
# Find all crashes in logs
Get-Content watchdog-notepad-*.log | Select-String "CRASH" -Context 3

# Count restarts
(Get-Content watchdog-notepad-*.log | Select-String "RESTART").Count
```

### 3. Backup Strategy
```powershell
# Archive all logs from previous month
$lastMonth = (Get-Date).AddMonths(-1)
Get-ChildItem watchdog-*.log | Where-Object {
    $_.LastWriteTime.Month -eq $lastMonth.Month
} | Copy-Item -Destination "\\backupserver\logs\"
```

### 4. Cleanup Schedule
Create a scheduled task to clean up old logs:

```batch
REM Run daily to remove logs older than 90 days
forfiles /S /M watchdog-*.log /D +90 /C "cmd /c del @path"
```

## Troubleshooting

### "Could not rotate log file" Warning

**Cause**: Permission issue or file lock
**Solution**: 
- Check folder permissions for current directory
- Ensure no other process is locking the log file
- Verify disk space is available

### Logs Growing Too Fast

**Scenario**: Logs reach 10 MB multiple times per day
**Solutions**:
1. Reduce application verbosity if possible
2. Check for excessive restart loops
3. Archive rotated logs more frequently
4. Increase monitoring for system issues

### Rotation Not Appearing

**Scenario**: No rotation messages in console
**Cause**: Application hasn't written logs since size/date threshold
**Expected Behavior**: Rotation happens on next log write after threshold is reached

### Missing Rotated Logs

**Scenario**: Expected .1, .2 files not found
**Cause**: Log file size never exceeded 10 MB and date didn't change
**Expected Behavior**: Current log file stays as `watchdog-{app}-{date}.log` until threshold

## Performance Impact

The log rotation check has **minimal performance impact**:

- ✅ File size check: ~1 microsecond (cached by OS)
- ✅ Date check: ~1 microsecond
- ✅ Rotation operation: ~10-100 milliseconds (infrequent)
- ✅ Overall: < 0.1% CPU impact

## Configuration

### Change Default Settings

To modify rotation behavior, edit **Program.cs**:

```csharp
// Change Maximum Log Size (currently 10 MB)
private const long MAX_LOG_SIZE = 10 * 1024 * 1024; // in bytes

// Example: Change to 50 MB
private const long MAX_LOG_SIZE = 50 * 1024 * 1024;
```

After modification, rebuild:
```powershell
dotnet build Watchdog.csproj
```

## Summary

| Feature | Details |
|---------|---------|
| **Size Limit** | 10 MB per log file |
| **Time Rotation** | Daily (at date change) |
| **Trigger** | Whichever comes first |
| **Frequency Check** | Before every log write |
| **Archive Format** | `{name}.{rotation-number}.log` |
| **Performance** | < 1 microsecond per check |
| **Disk Impact** | Prevents unbounded growth |
| **User Notification** | Console message on rotation |

---

**Version**: 2.1 (with Log Rotation Support)  
**Last Updated**: 2026-02-22
