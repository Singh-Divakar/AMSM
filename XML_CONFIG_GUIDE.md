# Watchdog Application - XML Configuration Guide

## Overview

The Watchdog application now supports managing multiple applications via XML configuration files. This allows centralized configuration of all monitored applications.

## Features

✅ **Multiple Applications** - Manage multiple applications in a single XML file  
✅ **Flexible Arguments** - Pass any arguments to each application  
✅ **Interactive Selection** - Choose which application to monitor from a menu  
✅ **Backward Compatible** - Still supports direct command-line usage  
✅ **Automatic Detection** - Loads `config.xml` automatically if it exists  

## XML Configuration Format

### Basic Structure

```xml
<?xml version="1.0" encoding="utf-8"?>
<Applications>
  <Application>
    <Name>Application Name</Name>
    <Path>C:\path\to\application.exe</Path>
    <Arguments>--arg1 value1 --arg2</Arguments>
  </Application>
</Applications>
```

### XML Element Descriptions

| Element | Description | Required | Example |
|---------|-------------|----------|---------|
| `Applications` | Root element containing all applications | Yes | `<Applications>...</Applications>` |
| `Application` | Single application configuration | Yes | `<Application>...</Application>` |
| `Name` | Display name for the application | Yes | `<Name>Notepad</Name>` |
| `Path` | Full or relative path to the executable | Yes | `<Path>notepad.exe</Path>` |
| `Arguments` | Command-line arguments (optional) | No | `<Arguments>--port 8080</Arguments>` |

## Usage Modes

### Mode 1: Automatic (config.xml)

If `config.xml` exists in the current directory, just run:

```powershell
Watchdog.exe
```

The watchdog will:
1. Load `config.xml`
2. If one application → Start monitoring automatically
3. If multiple applications → Show selection menu

### Mode 2: Specify Custom Config File

```powershell
Watchdog.exe myconfig.xml
```

Load from a custom XML configuration file.

### Mode 3: Direct Command Line (Backward Compatible)

```powershell
Watchdog.exe notepad.exe
Watchdog.exe "C:\MyApp\app.exe" --port 8080 --verbose
```

Use the traditional command-line approach (no XML required).

## Examples

### Example 1: Single Application in config.xml

**config.xml:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<Applications>
  <Application>
    <Name>Notepad</Name>
    <Path>notepad.exe</Path>
    <Arguments></Arguments>
  </Application>
</Applications>
```

**Usage:**
```powershell
Watchdog.exe
# Notepad starts automatically without menu
```

### Example 2: Multiple Applications with Menu

**config.xml:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<Applications>
  <Application>
    <Name>Notepad</Name>
    <Path>notepad.exe</Path>
    <Arguments></Arguments>
  </Application>
  
  <Application>
    <Name>Calculator</Name>
    <Path>C:\Windows\System32\calc.exe</Path>
    <Arguments></Arguments>
  </Application>
  
  <Application>
    <Name>Web Server</Name>
    <Path>C:\MyApp\server.exe</Path>
    <Arguments>--port 8080 --env production</Arguments>
  </Application>
</Applications>
```

**Usage:**
```powershell
Watchdog.exe
```

**Output:**
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

### Example 3: Complex Arguments

**config.xml:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<Applications>
  <Application>
    <Name>API Service</Name>
    <Path>C:\Services\api-service.exe</Path>
    <Arguments>--config "C:\config\api.json" --port 5000 --verbose</Arguments>
  </Application>
  
  <Application>
    <Name>Database</Name>
    <Path>C:\Database\db-server.exe</Path>
    <Arguments>--data-dir "C:\Data\DbFiles" --log-level info</Arguments>
  </Application>
</Applications>
```

### Example 4: Mixed Path Types

**config.xml:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<Applications>
  <!-- Relative path (searches in PATH) -->
  <Application>
    <Name>Notepad</Name>
    <Path>notepad.exe</Path>
    <Arguments></Arguments>
  </Application>
  
  <!-- Full path with spaces -->
  <Application>
    <Name>Program Files App</Name>
    <Path>C:\Program Files\MyApp\myapp.exe</Path>
    <Arguments></Arguments>
  </Application>
  
  <!-- Network path -->
  <Application>
    <Name>Network App</Name>
    <Path>\\server\share\app.exe</Path>
    <Arguments>--auto-start</Arguments>
  </Application>
</Applications>
```

## Command Line Examples

```powershell
# Use default config.xml in current directory
Watchdog.exe

# Use custom config file
Watchdog.exe applications.xml

# Use custom config with path
Watchdog.exe "C:\configs\my-apps.xml"

# Direct command line (no XML)
Watchdog.exe notepad.exe

# Direct with arguments
Watchdog.exe "C:\MyApp\server.exe" --port 8080
```

## Logging with XML Config

When using XML configuration, log files are created as:

```
watchdog-{application-name}-{date}.log
```

### Example Log File Names

- `watchdog-Notepad-2026-02-22.log`
- `watchdog-Calculator-2026-02-22.log`
- `watchdog-Web Server-2026-02-22.log`

Each application has its own separate log file with:
- Crash times with timestamps
- Process IDs (PID)
- Exit codes
- Restart attempts
- Time between crash and restart

## Creating Your Own config.xml

### Step 1: Identify Applications

List all applications you want to monitor:
- Path to executable
- Required arguments
- Display name

### Step 2: Create XML File

Create a file named `config.xml` (or any name.xml):

```xml
<?xml version="1.0" encoding="utf-8"?>
<Applications>
  <Application>
    <Name>App 1</Name>
    <Path>path\to\app1.exe</Path>
    <Arguments></Arguments>
  </Application>
  
  <Application>
    <Name>App 2</Name>
    <Path>path\to\app2.exe</Path>
    <Arguments>arg1 arg2 arg3</Arguments>
  </Application>
</Applications>
```

### Step 3: Run Watchdog

```powershell
Watchdog.exe config.xml
# or if named config.xml in current directory
Watchdog.exe
```

## Best Practices

### 1. Use Meaningful Names
```xml
<!-- Good -->
<Name>API Backend Service</Name>

<!-- Avoid -->
<Name>app1</Name>
```

### 2. Use Full Paths for Reliability
```xml
<!-- Better -->
<Path>C:\Services\myapp.exe</Path>

<!-- May fail if PATH isn't set correctly -->
<Path>myapp.exe</Path>
```

### 3. Quote Paths with Spaces
```xml
<!-- If needed in arguments -->
<Arguments>--config "C:\Program Files\Config\settings.json"</Arguments>
```

### 4. Keep Arguments Simple
```xml
<!-- Good: clear and simple -->
<Arguments>--port 8080 --env production</Arguments>

<!-- Avoid: complex quoting if possible -->
<Arguments>--path "C:\Very\Long\Path With Spaces\file.txt"</Arguments>
```

### 5. One Application Per File
```xml
<!-- Good: separate files for organization -->
config-webservers.xml
config-services.xml
config-utilities.xml
```

## Troubleshooting

### Error: "No applications found in config file"
- Check XML syntax is valid
- Ensure `<Application>` elements have `<Path>` specified
- Verify file encoding is UTF-8

### Error: "Failed to start application"
- Verify executable path is correct
- Check application exists at specified location
- Ensure you have permission to execute it

### Application not showing in menu
- Verify `<Path>` element is not empty
- Check XML file has valid structure
- Ensure application name is unique

### Config file not found
- When using `Watchdog.exe`, ensure `config.xml` is in current directory
- When using custom file: `Watchdog.exe path\to\config.xml`
- Check file extension is `.xml`

## Advanced Usage

### Multiple Config Files

Create different config files for different environments:

```powershell
# Development
Watchdog.exe config-dev.xml

# Staging
Watchdog.exe config-staging.xml

# Production
Watchdog.exe config-prod.xml
```

### Batch Processing

Create a script to monitor multiple configs:

```powershell
# monitor-all.ps1
Start-Process "f:\AMSM\bin\Debug\net6.0\Watchdog.exe" -ArgumentList "config-api.xml"
Start-Process "f:\AMSM\bin\Debug\net6.0\Watchdog.exe" -ArgumentList "config-services.xml"
Start-Process "f:\AMSM\bin\Debug\net6.0\Watchdog.exe" -ArgumentList "config-utils.xml"
```

### Network Shares

Point to applications on network shares (if accessible):

```xml
<Application>
  <Name>Shared Service</Name>
  <Path>\\networkserver\share\application.exe</Path>
  <Arguments>--auto-start</Arguments>
</Application>
```

## Summary

The XML configuration feature provides:
- **Central management** of multiple applications
- **Flexible configuration** with arguments
- **Interactive selection** for multiple apps
- **Backward compatibility** with command-line usage
- **Professional logging** with detailed crash information

Start with the provided `config.xml` template and customize it for your needs!

---

**Version**: 2.0 (with XML Configuration Support)  
**Updated**: 2026-02-22
