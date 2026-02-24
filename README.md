# AMSM — Application Monitor & Service Manager

A lightweight Python watchdog that monitors and manages Windows applications. It reads a configuration file, starts enabled applications, continuously monitors them, and **auto-restarts** any that crash — all with detailed logging.

## Features

- 📋 **XML Configuration** — Define applications in `config.xml` with name, path, arguments, and start flag
- 🔄 **Auto-Restart** — Detects crashed/exited processes and restarts them within seconds
- 📝 **Dual Logging** — Console output (INFO) + rotating log file (DEBUG, 5 MB × 3 backups)
- 🛑 **Graceful Shutdown** — `Ctrl+C` cleanly terminates all managed child processes
- 🧩 **Modular Design** — Clean multi-file package for easy maintenance and extensibility
- 📦 **Zero Dependencies** — Uses only the Python standard library

## Project Structure

```
AMSM/
├── config.xml                 # Application definitions
├── main.py                    # Entry point
├── README.md
└── amsm/                      # Core package
    ├── __init__.py
    ├── constants.py            # Paths, intervals, log settings
    ├── logger.py               # Dual console + file logging
    ├── config_parser.py        # XML parsing → AppConfig objects
    ├── process_manager.py      # Subprocess lifecycle (start/stop/poll)
    └── watchdog.py             # Monitor loop & orchestration
```

## Quick Start

### Prerequisites

- Python 3.10+

### Run

```bash
python main.py
```

The watchdog will:
1. Read `config.xml`
2. Start all applications with `<Start>true</Start>`
3. Poll every 5 seconds — restart any that have exited
4. Log all events to console and `watchdog.log`

Press **Ctrl+C** to stop and terminate all managed processes.

## Configuration

Edit `config.xml` to define your applications:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Applications>
  <Application>
    <Name>My App</Name>
    <Path>C:\path\to\app.exe</Path>
    <Arguments>--port 8080 --env production</Arguments>
    <Start>true</Start>
  </Application>
</Applications>
```

| Field        | Description                                      |
|--------------|--------------------------------------------------|
| `Name`       | Display name (used in log messages)              |
| `Path`       | Full path to the executable                      |
| `Arguments`  | Command-line arguments (space-separated)         |
| `Start`      | `true` to auto-start, `false` to skip            |

## Logs

Logs are written to both the console and `watchdog.log` in the project root.

```
2026-02-24 19:04:27 | INFO     | AMSM Watchdog starting
2026-02-24 19:04:28 | INFO     | Loaded 3 application(s) from config
2026-02-24 19:04:28 | INFO     | [Chrome] Started  (PID 12345)
2026-02-24 19:04:33 | WARNING  | [Chrome] Exited with code 0 — restarting …
2026-02-24 19:04:46 | INFO     | Received signal 2
2026-02-24 19:04:48 | INFO     | All processes terminated. Goodbye!
```

Log rotation: max **5 MB** per file, keeps **3** backups (`watchdog.log.1`, `.2`, `.3`).

## Customization

Key settings live in [`amsm/constants.py`](amsm/constants.py):

| Constant         | Default | Description                        |
|------------------|---------|------------------------------------|
| `POLL_INTERVAL`  | `5`     | Seconds between health checks      |
| `LOG_MAX_BYTES`  | `5 MB`  | Max size before log rotation       |
| `LOG_BACKUP_COUNT` | `3`   | Number of rotated log backups      |
