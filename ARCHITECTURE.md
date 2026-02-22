# Watchdog Application - Architecture & Execution Flow

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────┐
│                  WATCHDOG APPLICATION                │
│                                                       │
│  ┌────────────────────────────────────────────────┐  │
│  │           COMMAND LINE / XML LOADER             │  │
│  │  • Parse arguments                              │  │
│  │  • Load config.xml (if exists)                  │  │
│  │  • Show application menu (if multiple)          │  │
│  └────────────────────────────────────────────────┘  │
│                        │                              │
│                        ▼                              │
│  ┌────────────────────────────────────────────────┐  │
│  │         PROCESS MONITORING LOOP                 │  │
│  │  • Start executable                             │  │
│  │  • Log start event                              │  │
│  │  • Wait for process exit                        │  │
│  └────────────────────────────────────────────────┘  │
│                        │                              │
│                        ▼                              │
│  ┌────────────────────────────────────────────────┐  │
│  │        CRASH DETECTION & RESTART                │  │
│  │  • Detect exit                                  │  │
│  │  • Log crash with timestamp                     │  │
│  │  • Wait 2 seconds                               │  │
│  │  • Restart application                          │  │
│  │  • Log restart time                             │  │
│  │  • Go back to monitoring                        │  │
│  └────────────────────────────────────────────────┘  │
│                        │                              │
│                        ▼                              │
│  ┌────────────────────────────────────────────────┐  │
│  │         LOGGING SUBSYSTEM                       │  │
│  │  • Create log file: watchdog-{app}-{date}.log   │  │
│  │  • Write all events with timestamps             │  │
│  │  • Track crashes, restarts, and statistics      │  │
│  │  • Auto-flush for data safety                   │  │
│  └────────────────────────────────────────────────┘  │
│                        │                              │
│                        ▼                              │
│  ┌────────────────────────────────────────────────┐  │
│  │        GRACEFUL SHUTDOWN (Ctrl+C)               │  │
│  │  • Receive interrupt signal                      │  │
│  │  • Terminate monitored process                  │  │
│  │  • Display final statistics                     │  │
│  │  • Close log file                               │  │
│  │  • Exit cleanly                                 │  │
│  └────────────────────────────────────────────────┘  │
│                                                       │
└─────────────────────────────────────────────────────┘
```

## 📋 Execution Flow

### Step 1: Application Startup

```
┌─────────────────────────┐
│  Watchdog.exe started   │
└────────────┬────────────┘
             │
             ▼
┌──────────────────────────────────┐
│ Parse Command Line Arguments     │
├──────────────────────────────────┤
│ Case 1: No arguments             │ ──► Load config.xml if exists
│ Case 2: *.xml argument           │ ──► Load specified XML file
│ Case 3: *.exe argument           │ ──► Use direct path
│ Case 4: Unknown                  │ ──► Show help and exit
└─────────────────────────────────┘
```

### Step 2: Application Selection

```
If Multiple Applications in XML:
┌────────────────────────────┐
│ Show Interactive Menu       │
├────────────────────────────┤
│ 1) Application A            │
│ 2) Application B            │
│ 3) Application C            │
│ Q) Quit                     │
└────────────────────────────┘
         │
         ▼
   User Selects (1-3)
         │
         ▼
   ✅ Application Selected
         │
         ▼
  Proceed to Startup


If Single Application or Direct CLI:
   └───► ✅ Skip menu, proceed to startup
```

### Step 3: Process Startup

```
┌────────────────────────────────────┐
│ Show Startup Information            │
├────────────────────────────────────┤
│ • Application name                  │
│ • Executable path                   │
│ • Arguments                         │
│ • Log file location                 │
└────────┬───────────────────────────┘
         │
         ▼
┌─────────────────────────────┐
│ Start Monitored Process      │
├─────────────────────────────┤
│ • Create ProcessStartInfo    │
│ • Set executable path        │
│ • Set arguments              │
│ • Start process              │
│ • Capture PID                │
└─────────┬───────────────────┘
          │
          ▼
┌──────────────────────────┐
│ Log Application Started  │
├──────────────────────────┤
│ [START] HH:mm:ss         │
│ Application: {name}      │
│ PID: {process_id}        │
└──────────┬───────────────┘
           │
           ▼
   ✅ Continue Monitoring
```

### Step 4: Monitoring Loop

```
┌────────────────────────────────┐
│  MONITORING LOOP (Continuous)  │
├────────────────────────────────┤
│                                │
│  ┌──────────────────────────┐  │
│  │ Wait for Process Exit    │  │
│  │ (WaitForExit())          │  │
│  └──────────┬───────────────┘  │
│             │                   │
│             ▼                   │
│  ┌──────────────────────────┐  │
│  │ Process Exited?          │  │
│  │ (Check HasExited)        │  │
│  └──────┬─────────┬─────────┘  │
│         │         │             │
│        NO        YES            │
│         │         │             │
│         │         ▼             │
│         │   ┌─────────────────┐ │
│         │   │ Crash Detected  │ │
│         │   └────────┬────────┘ │
│         │            │          │
│         ▼            ▼          │
│      Stay         [Next Step]   │
│      Here         (Restart)     │
│                                │
└────────────────────────────────┘
```

### Step 5: Crash Handling & Restart

```
┌──────────────────────────────────┐
│  APPLICATION CRASHED/EXITED      │
├──────────────────────────────────┤
│                                  │
│  Get crash time                  │
│  Get exit code                   │
│  Get process ID                  │
│                                  │
└────────────┬─────────────────────┘
             │
             ▼
┌──────────────────────────────────────┐
│  LOG CRASH EVENT                     │
├──────────────────────────────────────┤
│ [CRASH] 2026-02-22 10:31:12          │
│ Application: notepad                 │
│ PID: 5432                            │
│ Exit Code: 0                         │
│ Restart Attempt: #1                  │
└────────────┬──────────────────────────┘
             │
             ▼
┌──────────────────────────────────────┐
│  DISPLAY CRASH NOTIFICATION          │
├──────────────────────────────────────┤
│ ⚠️  CRASH DETECTED                   │
│ 🔴 Application 'notepad' crashed     │
│ 📊 Exit Code: 0 | PID: 5432          │
│ 🔄 Restarting... (Attempt #1)        │
└────────────┬──────────────────────────┘
             │
             ▼
┌──────────────────────────────────────┐
│  WAIT 2 SECONDS                      │
│  (Prevent rapid restart loop)        │
└────────────┬──────────────────────────┘
             │
             ▼
┌──────────────────────────────────────┐
│  LOG RESTART EVENT                   │
├──────────────────────────────────────┤
│ [RESTART] 2026-02-22 10:31:14        │
│ Time between crash and restart: 2.0s │
└────────────┬──────────────────────────┘
             │
             ▼
┌──────────────────────────────────────┐
│  DISPLAY RESTART NOTIFICATION        │
├──────────────────────────────────────┤
│ 🚀 Restarting 'notepad' now...       │
└────────────┬──────────────────────────┘
             │
             ▼
  [Go Back to Step 3: Process Startup]
```

### Step 6: Graceful Shutdown (Ctrl+C)

```
User Presses Ctrl+C
       │
       ▼
┌──────────────────────────────────┐
│ Signal Received: SIGINT           │
│ (Console.CancelKeyPress)          │
└────────────┬─────────────────────┘
             │
             ▼
┌──────────────────────────────────┐
│ Set _isRunning = false            │
│ (Stop monitoring)                 │
└────────────┬─────────────────────┘
             │
             ▼
┌──────────────────────────────────┐
│ Log Termination Event             │
├──────────────────────────────────┤
│ [TERMINATION] HH:mm:ss            │
│ Terminating 'notepad' (PID: 5432) │
└────────────┬─────────────────────┘
             │
             ▼
┌──────────────────────────────────┐
│ Kill Monitored Process            │
│ (Process.Kill(true))              │
│ [Kills entire process tree]       │
└────────────┬─────────────────────┘
             │
             ▼
┌──────────────────────────────────┐
│ Display Final Statistics          │
├──────────────────────────────────┤
│ Watchdog Stopped                  │
│ Total Restarts: N                 │
│ Stopped at: HH:mm:ss              │
└────────────┬─────────────────────┘
             │
             ▼
┌──────────────────────────────────┐
│ Close Log File                    │
│ Clean Up Resources                │
└────────────┬─────────────────────┘
             │
             ▼
┌──────────────────────────────────┐
│ Exit Application                  │
│ Return to Shell                   │
└──────────────────────────────────┘
```

## 📂 File Structure & Dependencies

```
WatchdogApp (Program.cs)
├── System.Diagnostics (Process control)
├── System.IO (File logging)
├── System.Xml.Linq (XML parsing)
├── System.Threading.Tasks (Async operations)
└── System.Collections.Generic (List management)
     │
     ├─► Process Management
     │   ├── StartApplication()
     │   ├── WaitForExit()
     │   ├── Kill()
     │   └── Exit Code Tracking
     │
     ├─► XML Configuration
     │   ├── LoadApplicationsFromConfig()
     │   ├── SelectApplicationFromConfig()
     │   └── XDocument.Load()
     │
     └─► Logging System
         ├── LogToFile()
         ├── StreamWriter (file output)
         └── Timestamped entries
```

## 🔄 Main Loop (Conceptual)

```csharp
while (_isRunning)
{
    try
    {
        // 1. Start application
        Process process = StartApplication(path, args);
        
        // 2. Monitor process
        process.WaitForExit();  // Blocking until exit
        
        // 3. Detect crash
        int exitCode = process.ExitCode;
        LogCrash(exitCode);
        
        // 4. Prevent rapid restart
        await Task.Delay(2000);
        
        // 5. Loop back to step 1 (restart)
    }
    catch (Exception ex)
    {
        // Handle errors and retry
        LogError(ex);
        await Task.Delay(2000);
    }
}
```

## 📊 State Diagram

```
              ┌──────────────────┐
              │   Application    │
              │    Startup       │
              └────────┬─────────┘
                       │
                       ▼
        ┌──────────────────────────┐
        │   MONITORING             │
        │   (Waiting for exit)     │
        │                          │
        │ ┌──────────────────────┐ │
        │ │ Loop until process   │ │
        │ │ exits or Ctrl+C      │ │
        │ └──────────────────────┘ │
        └────────┬──────────┬───────┘
                 │          │
            Crashed      Ctrl+C
                 │          │
                 ▼          ▼
        ┌──────────────┐ ┌──────────────┐
        │   RESTART    │ │  SHUTDOWN    │
        │   (Wait 2s)  │ │  (Terminate) │
        │   Then loop  │ │  Then exit   │
        └──────┬───────┘ └──────────────┘
               │
               └─────────┬─────────┘
                         │
                         ▼
              ┌──────────────────┐
              │   Application    │
              │    Startup       │
              └──────────────────┘
```

## 🎯 Key Decision Points

### 1. How to Run?
```
Is config.xml present? ──► YES ──► Load XML
                          │
                          NO
                          │
                          ▼
Is argument *.xml? ──► YES ──► Load XML file
                   │
                   NO
                   │
                   ▼
Is argument *.exe? ──► YES ──► Use direct path
                   │
                   NO
                   │
                   ▼
Show Help & Exit
```

### 2. How Many Apps in XML?
```
Applications found = 1? ──► YES ──► Use directly
                           │
                           NO
                           │
                           ▼
Show Selection Menu
```

### 3. Process Status?
```
Process running?     ──► YES ──► Continue waiting
                     │
                     NO
                     │
                     ▼
Increment restart counter
Log crash event
Wait 2 seconds
Restart process
```

## 🔐 Thread Safety

```
┌─────────────────────────────────────┐
│    Thread-Safe Process Management   │
├─────────────────────────────────────┤
│                                     │
│  Lock (_lockObject)                 │
│  ├── Start new process              │
│  ├── Check process state            │
│  ├── Terminate process              │
│  └── Dispose process                │
│                                     │
│  Lock ensures:                      │
│  ├── Only one thread modifies       │
│  ├── No race conditions             │
│  ├── Safe resource cleanup          │
│  └── Consistent state               │
│                                     │
└─────────────────────────────────────┘
```

## 📝 Log File Format

```
Time Structure:
┌──────────────────────────────────┐
│ Event Timestamp (Entry)          │
│ [YYYY-MM-DD HH:mm:ss]            │
│                                  │
│ Associated Timestamps            │
│ • Crash time (separate entry)    │
│ • Restart time (separate entry)  │
│ • Time delta (restart attempt)   │
└──────────────────────────────────┘

Entry Types:
┌──────────────────────────────────┐
│ [START]       - App started      │
│ [CRASH]       - App crashed      │
│ [RESTART]     - App restarting   │
│ [TERMINATION] - App terminating  │
│ [ERROR]       - Error occurred   │
└──────────────────────────────────┘
```

## 🎯 Performance Considerations

1. **Process Monitoring**: Uses blocking `WaitForExit()` - efficient for single process
2. **Memory Usage**: Minimal (~50 MB including .NET runtime)
3. **CPU Usage**: Minimal (0% when waiting, brief spike on restart)
4. **Logging**: Buffered writes with auto-flush
5. **Restart Timeout**: 2 seconds delay prevents resource exhaustion

## ✅ Implementation Completeness

- ✅ Process spawning and monitoring
- ✅ Crash detection and handling
- ✅ Automatic restart mechanism
- ✅ Graceful shutdown with Ctrl+C
- ✅ File-based logging with timestamps
- ✅ XML configuration support
- ✅ Interactive application menu
- ✅ Comprehensive error handling
- ✅ Thread-safe operations
- ✅ Resource cleanup

---

**This architecture ensures:**
- Reliable process monitoring
- Automatic recovery from crashes
- User-friendly operations
- Production-ready logging
- Extensible design

