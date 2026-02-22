# Testing the XML Configuration Feature

This document explains how to test the new XML configuration feature.

## Test 1: Verify config.xml loads

### Setup
```powershell
cd f:\AMSM
```

### Run
```powershell
f:\AMSM\bin\Debug\net6.0\Watchdog.exe
```

### Expected Behavior
- If config.xml exists and has one application → Starts monitoring automatically
- If config.xml exists and has multiple applications → Shows menu to select

## Test 2: Use custom XML config file

### Setup
Create your own `myapps.xml` with applications

### Run
```powershell
f:\AMSM\bin\Debug\net6.0\Watchdog.exe myapps.xml
```

### Expected Behavior
- Loads applications from myapps.xml
- Shows selection menu if multiple apps
- Starts monitoring selected application

## Test 3: Backward compatibility (direct command line)

### Run
```powershell
f:\AMSM\bin\Debug\net6.0\Watchdog.exe notepad.exe
```

### Expected Behavior
- Works exactly as before (no XML needed)
- Notepad starts and is monitored
- Close notepad → it restarts automatically

## Test 4: With arguments

### Run
```powershell
f:\AMSM\bin\Debug\net6.0\Watchdog.exe notepad.exe
```

Or using XML config with arguments defined:

```xml
<Application>
  <Name>Notepad with Document</Name>
  <Path>notepad.exe</Path>
  <Arguments>document.txt</Arguments>
</Application>
```

### Expected Behavior
- Application starts with specified arguments
- Restarts with same arguments on crash

## Test 5: Menu selection

### Files required
- config.xml with 3+ applications

### Run
```powershell
Watchdog.exe
```

### Expected Behavior
```
╔════════════════════════════════════════════════╗
║      Select Application to Monitor             ║
╚════════════════════════════════════════════════╝

  1) Notepad
     📁 Path: notepad.exe

  2) Calculator
     📁 Path: C:\Windows\System32\calc.exe

  3) Web Server
     📁 Path: C:\MyApp\server.exe
     ⚙️  Args: --port 8080 --env production

  Q) Quit

Enter your choice (1-3 or Q):
```

Type 1, 2, or 3 to select application

## Test 6: Logging

### Setup
- Run watchdog with any application

### Expected Behavior
- Log file created: `watchdog-{appname}-{date}.log`
- Logs contain:
  - Start timestamp
  - Application name
  - PID
  - Crash time
  - Exit code
  - Restart attempts

### Verify
```powershell
Get-ChildItem f:\AMSM\watchdog-*.log
Get-Content f:\AMSM\watchdog-notepad-2026-02-22.log
```

## Quick Test Script

```powershell
# Test 1: Show current executable
Write-Host "✅ Test 1: Executable location"
$exe = "f:\AMSM\bin\Debug\net6.0\Watchdog.exe"
if (Test-Path $exe) { Write-Host "Found: $exe" } else { Write-Host "NOT FOUND" }

# Test 2: Show config file
Write-Host "`n✅ Test 2: Config file"
$config = "f:\AMSM\config.xml"
if (Test-Path $config) { Write-Host "Found: $config" } else { Write-Host "NOT FOUND" }

# Test 3: Show sample content
Write-Host "`n✅ Test 3: Config file content"
Get-Content $config | Select-Object -First 15

# Test 4: List log files
Write-Host "`n✅ Test 4: Log files"
Get-ChildItem f:\AMSM\watchdog-*.log | Select-Object Name, Length | Format-Table
```

## Running the Tests

1. **Test without XML** (backward compatible):
   ```powershell
   f:\AMSM\bin\Debug\net6.0\Watchdog.exe notepad.exe
   ```
   Close notepad → should restart

2. **Test with XML** (single app):
   ```powershell
   f:\AMSM\bin\Debug\net6.0\Watchdog.exe config.xml
   ```
   Should show single app or menu

3. **Test with automatic XML** (place config.xml in working directory):
   ```powershell
   cd f:\AMSM
   f:\AMSM\bin\Debug\net6.0\Watchdog.exe
   ```
   Should load config.xml automatically

## Expected Results

All tests should show:
- ✅ Application starts
- ✅ Process ID logged
- ✅ Timestamped console output
- ✅ Log file created
- ✅ Close application → restarts
- ✅ Ctrl+C → graceful shutdown
- ✅ Log shows crash and restart times
