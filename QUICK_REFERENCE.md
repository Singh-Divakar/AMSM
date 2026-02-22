# Watchdog Application - Quick Reference

## 📦 Project Files

```
f:\AMSM\
├── 📄 Program.cs                      Main application code (XML + logging)
├── 📄 Watchdog.csproj                 Project configuration (.NET 6.0)
├── 📄 config.xml                      Example XML configuration file
│
├── 📚 Documentation
│   ├── README.md                      Complete feature documentation
│   ├── QUICKSTART.md                  Getting started guide
│   ├── XML_CONFIG_GUIDE.md            XML configuration guide
│   ├── PROJECT_SUMMARY.md             Project overview
│   ├── TESTING_XML_FEATURE.md         XML testing guide
│   └── ADVANCED_EXAMPLES.cs           Customization code snippets
│
├── 🔧 Build Scripts
│   ├── build-and-run.bat              Windows batch build script
│   └── examples.sh                    Shell script examples
│
├── 🧪 Testing
│   └── test-watchdog.ps1              Interactive test suite
│
└── 📁 bin/Debug/net6.0/
    └── Watchdog.exe                   ✅ COMPILED & READY TO USE
```

## 🚀 Three Ways to Use

### 1️⃣ Direct Command Line (Simplest)
```powershell
Watchdog.exe notepad.exe
Watchdog.exe "C:\MyApp\app.exe" --port 8080
```

### 2️⃣ XML Configuration (Single App)
```powershell
Watchdog.exe config.xml
```

### 3️⃣ XML with Menu Selection (Multiple Apps)
Create config.xml with multiple applications, then:
```powershell
Watchdog.exe config.xml
# or if in same directory:
Watchdog.exe
```

## 📝 config.xml Template

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

## 🎯 Key Features at a Glance

| Feature | Direct CLI | XML Single | XML Multiple |
|---------|-----------|-----------|-------------|
| Simple usage | ✅ | ✅ | ✅ |
| Pass arguments | ✅ | ✅ | ✅ |
| Multiple apps | ❌ | ❌ | ✅ |
| Selection menu | ❌ | ❌ | ✅ |
| Centralized config | ❌ | ✅ | ✅ |
| Logging | ✅ | ✅ | ✅ |
| Log rotation | ✅ | ✅ | ✅ |
| Auto-restart | ✅ | ✅ | ✅ |

## 📊 Logging

Each run creates a timestamped log:
```
watchdog-{app-name}-{date}.log
```

Example: `watchdog-notepad-2026-02-22.log`

### Log Rotation

Logs automatically rotate at **10 MB** OR **daily** (whichever comes first):

**Rotated logs** are archived as:
```
watchdog-{app-name}-{date}.1.log    (daily rotation)
watchdog-{app-name}-{date-time}.1.log (size rotation)
```

**Console notification:**
```
[HH:mm:ss] 📦 Log rotated (size) -> watchdog-notepad-2026-02-22-143025.1.log
[HH:mm:ss] 📦 Log rotated (daily) -> watchdog-notepad-2026-02-21.1.log
```

See [LOG_ROTATION_GUIDE.md](LOG_ROTATION_GUIDE.md) for complete rotation details.

## 🛑 Stopping

Press **Ctrl+C** in the console to:
1. Stop accepting new responsibilities
2. Gracefully terminate the monitored application
3. Display final statistics
4. Exit cleanly

## ⚙️ Building from Source

```powershell
cd f:\AMSM
dotnet build Watchdog.csproj
# or for release
dotnet build --configuration Release
```

## 📦 Executables

- **Debug**: `f:\AMSM\bin\Debug\net6.0\Watchdog.exe`
- **Release**: `f:\AMSM\bin\Release\net6.0\Watchdog.exe`

## 🧪 Testing Quick Commands

```powershell
# Test 1: Help message
Watchdog.exe

# Test 2: Monitor Notepad (close to see restart)
Watchdog.exe notepad.exe

# Test 3: Load from XML
Watchdog.exe config.xml

# Test 4: Auto-load config.xml from current directory
cd f:\AMSM
Watchdog.exe
```

## 📋 Command Syntax Quick Reference

```powershell
# No arguments - show help
Watchdog.exe

# Direct executable
Watchdog.exe <path.exe>

# Direct with arguments
Watchdog.exe <path.exe> arg1 arg2

# XML file
Watchdog.exe <config.xml>

# Auto-detect config.xml
Watchdog.exe
```

## 🔗 Related Documentation

| Document | Purpose |
|----------|---------|
| [README.md](README.md) | Complete feature reference |
| [QUICKSTART.md](QUICKSTART.md) | Getting started in 5 minutes |
| [XML_CONFIG_GUIDE.md](XML_CONFIG_GUIDE.md) | Detailed XML configuration help |
| [TESTING_XML_FEATURE.md](TESTING_XML_FEATURE.md) | XML feature testing guide |
| [ADVANCED_EXAMPLES.cs](ADVANCED_EXAMPLES.cs) | Code customization examples |

## 💡 Tips & Tricks

### Create a Shortcut
Add to PATH or create a desktop shortcut pointing to `Watchdog.exe`

### Multiple Watchdogs
Run multiple instances with different configs:
```powershell
Start-Process Watchdog.exe -ArgumentList "config-api.xml"
Start-Process Watchdog.exe -ArgumentList "config-db.xml"
```

### Log File Analysis
```powershell
# View latest log
Get-ChildItem watchdog-*.log | Sort-Object LastWriteTime -Desc | Select-Object -First 1 | Get-Content

# Search for crashes
Get-Content watchdog-*.log | Select-String "CRASH"

# Count restarts in a log
(Get-Content watchdog-app-*.log | Select-String "RESTART").Count
```

## ❓ Common Questions

**Q: How do I monitor multiple apps at once?**  
A: Create a config.xml with multiple applications, then run `Watchdog.exe config.xml`

**Q: Can I change the restart delay?**  
A: Yes! Modify the `2000` in `Task.Delay(2000)` in Program.cs (in milliseconds)

**Q: Where are the log files?**  
A: In the current working directory as `watchdog-{appname}-{date}.log`

**Q: Can I use relative paths in the XML?**  
A: Yes, both absolute (`C:\...`) and relative (`.\app.exe`) paths work

**Q: What if config.xml has errors?**  
A: The app will display an error message and exit. Check XML syntax in a text editor.

## 🔐 Permissions Required

- Read: Execute access to the monitored application
- Write: Permission to create log files in current directory
- Admin: May be needed to kill certain processes on shutdown

---

**Version**: 2.0 (with XML Configuration Support)  
**Status**: ✅ Production Ready  
**Last Updated**: 2026-02-22
