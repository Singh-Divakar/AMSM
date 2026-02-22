using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

class WatchdogApp
{
    private Process? _watchedProcess;
    private bool _isRunning = true;
    private int _restartCount = 0;
    private readonly object _lockObject = new object();
    private StreamWriter? _logFile;
    private string _applicationName = "";
    private string _logFilePath = "";
    private DateTime _lastLogDate = DateTime.Now;
    private int _logRotationNumber = 0;
    private const long MAX_LOG_SIZE = 10 * 1024 * 1024; // 10 MB

    static async Task Main(string[] args)
    {
        string executablePath = "";
        string arguments = "";

        // Check if XML config file is provided or if default exists
        if (args.Length == 0 && File.Exists("config.xml"))
        {
            // Use default config.xml
            var selectedApp = SelectApplicationFromConfig("config.xml");
            if (selectedApp == null)
            {
                Environment.Exit(0);
            }
            executablePath = selectedApp.Value.Path;
            arguments = selectedApp.Value.Arguments;
        }
        else if (args.Length >= 1 && args[0].EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
        {
            // XML config file provided
            if (!File.Exists(args[0]))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Config file not found: {args[0]}");
                Console.ResetColor();
                Environment.Exit(1);
            }
            var selectedApp = SelectApplicationFromConfig(args[0]);
            if (selectedApp == null)
            {
                Environment.Exit(0);
            }
            executablePath = selectedApp.Value.Path;
            arguments = selectedApp.Value.Arguments;
        }
        else if (args.Length >= 1)
        {
            // Traditional command line usage
            executablePath = args[0];
            arguments = args.Length > 1 ? string.Join(" ", args, 1, args.Length - 1) : "";
        }
        else
        {
            PrintUsage();
            Environment.Exit(1);
        }

        var watchdog = new WatchdogApp(executablePath);
        
        Console.WriteLine("╔════════════════════════════════════════════════╗");
        Console.WriteLine("║         Watchdog Application Started           ║");
        Console.WriteLine("╚════════════════════════════════════════════════╝");
        Console.WriteLine($"📁 Executable: {executablePath}");
        Console.WriteLine($"📋 Application: {watchdog._applicationName}");
        Console.WriteLine($"⚙️  Arguments: {(string.IsNullOrEmpty(arguments) ? "(none)" : arguments)}");
        Console.WriteLine($"⏰ Started at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"📝 Log file: {watchdog._logFilePath}");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("Press Ctrl+C to stop the watchdog.\n");

        watchdog.LogToFile($"═══════════════════════════════════════════════════");
        watchdog.LogToFile($"Watchdog Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        watchdog.LogToFile($"Application: {watchdog._applicationName}");
        watchdog.LogToFile($"Executable: {executablePath}");
        watchdog.LogToFile($"Arguments: {(string.IsNullOrEmpty(arguments) ? "(none)" : arguments)}");
        watchdog.LogToFile($"═══════════════════════════════════════════════════");

        // Handle Ctrl+C gracefully
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            watchdog.StopWatching();
        };

        try
        {
            await watchdog.WatchApplicationAsync(executablePath, arguments);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] Watchdog encountered an error: {ex.Message}");
            Console.ResetColor();
            watchdog.LogToFile($"[ERROR] Watchdog encountered an error: {ex.Message}");
            Environment.Exit(1);
        }
        finally
        {
            watchdog.Cleanup();
        }
    }

    // Constructor to initialize application name and log file
    public WatchdogApp(string executablePath)
    {
        _applicationName = Path.GetFileNameWithoutExtension(executablePath);
        _lastLogDate = DateTime.Now;
        _logRotationNumber = 0;
        _logFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"watchdog-{_applicationName}-{DateTime.Now:yyyy-MM-dd}.log");
        
        try
        {
            _logFile = new StreamWriter(_logFilePath, append: true)
            {
                AutoFlush = true
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not create log file: {ex.Message}");
        }
    }

    // Structure to hold application info
    private struct ApplicationInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Arguments { get; set; }
    }

    // Load applications from XML config file
    private static List<ApplicationInfo> LoadApplicationsFromConfig(string configPath)
    {
        var applications = new List<ApplicationInfo>();

        try
        {
            XDocument doc = XDocument.Load(configPath);
            var apps = doc.Root?.Elements("Application");

            if (apps != null)
            {
                foreach (var app in apps)
                {
                    var name = app.Element("Name")?.Value ?? "";
                    var path = app.Element("Path")?.Value ?? "";
                    var args = app.Element("Arguments")?.Value ?? "";

                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        applications.Add(new ApplicationInfo
                        {
                            Name = name,
                            Path = path,
                            Arguments = args
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error loading config file: {ex.Message}");
            Console.ResetColor();
        }

        return applications;
    }

    // Display menu and select application from config
    private static ApplicationInfo? SelectApplicationFromConfig(string configPath)
    {
        var applications = LoadApplicationsFromConfig(configPath);

        if (applications.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ No applications found in config file.");
            Console.ResetColor();
            return null;
        }

        if (applications.Count == 1)
        {
            // Only one application, use it directly
            return applications[0];
        }

        // Multiple applications, show menu
        Console.WriteLine("\n╔════════════════════════════════════════════════╗");
        Console.WriteLine("║      Select Application to Monitor             ║");
        Console.WriteLine("╚════════════════════════════════════════════════╝\n");

        for (int i = 0; i < applications.Count; i++)
        {
            Console.WriteLine($"  {i + 1}) {applications[i].Name}");
            Console.WriteLine($"     📁 Path: {applications[i].Path}");
            if (!string.IsNullOrWhiteSpace(applications[i].Arguments))
            {
                Console.WriteLine($"     ⚙️  Args: {applications[i].Arguments}");
            }
            Console.WriteLine();
        }

        Console.WriteLine($"  Q) Quit");
        Console.WriteLine();

        while (true)
        {
            Console.Write("Enter your choice (1-" + applications.Count + " or Q): ");
            string? choice = Console.ReadLine();

            if (choice?.ToUpper() == "Q")
            {
                return null;
            }

            if (int.TryParse(choice, out int index) && index > 0 && index <= applications.Count)
            {
                return applications[index - 1];
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Invalid choice. Please try again.");
            Console.ResetColor();
        }
    }

    private async Task WatchApplicationAsync(string executablePath, string arguments)
    {
        while (_isRunning)
        {
            try
            {
                lock (_lockObject)
                {
                    _watchedProcess = StartApplication(executablePath, arguments);
                }

                if (_watchedProcess != null)
                {
                    // Wait for the process to exit
                    _watchedProcess.WaitForExit();

                    int exitCode = _watchedProcess.ExitCode;
                    int pid = _watchedProcess.Id;
                    _watchedProcess.Dispose();
                    _watchedProcess = null;

                    if (_isRunning)
                    {
                        _restartCount++;
                        DateTime crashTime = DateTime.Now;
                        
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[{crashTime:HH:mm:ss}] ⚠️  CRASH DETECTED");
                        Console.WriteLine($"[{crashTime:HH:mm:ss}] 🔴 Application '{_applicationName}' crashed/exited");
                        Console.WriteLine($"[{crashTime:HH:mm:ss}] 📊 Exit Code: {exitCode} | PID: {pid}");
                        Console.WriteLine($"[{crashTime:HH:mm:ss}] 🔄 Restarting application (Attempt #{_restartCount})...");
                        Console.ResetColor();

                        // Log crash details
                        LogToFile($"");
                        LogToFile($"[CRASH] {crashTime:yyyy-MM-dd HH:mm:ss}");
                        LogToFile($"Application: {_applicationName}");
                        LogToFile($"PID: {pid}");
                        LogToFile($"Exit Code: {exitCode}");
                        LogToFile($"Restart Attempt: #{_restartCount}");
                        
                        // Wait 2 seconds before restarting to avoid rapid restart loops
                        await Task.Delay(2000);
                        
                        DateTime restartTime = DateTime.Now;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"[{restartTime:HH:mm:ss}] 🚀 Restarting '{_applicationName}' now...");
                        Console.ResetColor();
                        
                        LogToFile($"[RESTART] {restartTime:yyyy-MM-dd HH:mm:ss}");
                        LogToFile($"Time between crash and restart: {(restartTime - crashTime).TotalSeconds:F1} seconds");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ❌ Error: {ex.Message}");
                Console.ResetColor();
                LogToFile($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {ex.Message}");

                lock (_lockObject)
                {
                    _watchedProcess?.Dispose();
                    _watchedProcess = null;
                }

                if (_isRunning)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ⏳ Retrying in 2 seconds...");
                    await Task.Delay(2000);
                }
            }
        }

        Console.WriteLine("\n╔════════════════════════════════════════════════╗");
        Console.WriteLine("║       Watchdog Application Stopped             ║");
        Console.WriteLine("╚════════════════════════════════════════════════╝");
        Console.WriteLine($"✅ Total restarts: {_restartCount}");
        Console.WriteLine($"🛑 Stopped at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        
        LogToFile($"");
        LogToFile($"Watchdog Stopped: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        LogToFile($"Total Restarts: {_restartCount}");
        LogToFile($"═══════════════════════════════════════════════════");
    }

    private Process? StartApplication(string executablePath, string arguments)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = arguments,
                UseShellExecute = true,
                CreateNoWindow = false
            };

            Process process = Process.Start(processInfo)!;
            
            DateTime startTime = DateTime.Now;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[{startTime:HH:mm:ss}] ✅ Application started (PID: {process.Id})");
            Console.ResetColor();

            LogToFile($"[START] {startTime:yyyy-MM-dd HH:mm:ss}");
            LogToFile($"Application: {_applicationName}");
            LogToFile($"PID: {process.Id}");
            
            return process;
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ❌ Failed to start application: {ex.Message}");
            Console.WriteLine($"   Check if the executable path is correct: {executablePath}");
            Console.ResetColor();
            LogToFile($"[ERROR] Failed to start '{_applicationName}': {ex.Message}");
            throw;
        }
    }

    private void StopWatching()
    {
        _isRunning = false;
        
        lock (_lockObject)
        {
            if (_watchedProcess != null && !_watchedProcess.HasExited)
            {
                try
                {
                    DateTime stopTime = DateTime.Now;
                    Console.WriteLine($"\n[{stopTime:HH:mm:ss}] 🛑 Stopping watchdog. Terminating application...");
                    LogToFile($"[TERMINATION] {stopTime:yyyy-MM-dd HH:mm:ss}");
                    LogToFile($"Terminating '{_applicationName}' (PID: {_watchedProcess.Id})");
                    
                    _watchedProcess.Kill(true); // Kill process tree
                    _watchedProcess.WaitForExit(5000);
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✅ Application terminated.");
                    
                    LogToFile($"Application terminated successfully");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ❌ Error terminating application: {ex.Message}");
                    Console.ResetColor();
                    LogToFile($"[ERROR] Failed to terminate: {ex.Message}");
                }
                finally
                {
                    _watchedProcess.Dispose();
                    _watchedProcess = null;
                }
            }
        }
    }

    // Log messages to both console and file
    // Check and rotate log file if needed (10MB or daily rotation)
    private void CheckAndRotateLogFile()
    {
        try
        {
            // Check if date changed (daily rotation)
            if (DateTime.Now.Date != _lastLogDate.Date)
            {
                RotateLogFile("daily");
                _lastLogDate = DateTime.Now;
                _logRotationNumber = 0;
                return;
            }

            // Check if file size exceeds 10 MB
            if (File.Exists(_logFilePath))
            {
                FileInfo fileInfo = new FileInfo(_logFilePath);
                if (fileInfo.Length >= MAX_LOG_SIZE)
                {
                    RotateLogFile("size");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not check log rotation: {ex.Message}");
        }
    }

    // Rotate log file
    private void RotateLogFile(string reason)
    {
        try
        {
            // Close current log file
            _logFile?.Flush();
            _logFile?.Dispose();
            _logFile = null;

            // Archive current log file
            string archivePath;
            if (reason == "daily")
            {
                // Rename to include rotation number if multiple rotations in same day
                _logRotationNumber++;
                string archiveName = $"watchdog-{_applicationName}-{_lastLogDate:yyyy-MM-dd}.{_logRotationNumber}.log";
                archivePath = Path.Combine(Directory.GetCurrentDirectory(), archiveName);
            }
            else // size rotation
            {
                // Rename to include timestamp
                _logRotationNumber++;
                string archiveName = $"watchdog-{_applicationName}-{DateTime.Now:yyyy-MM-dd-HHmmss}.{_logRotationNumber}.log";
                archivePath = Path.Combine(Directory.GetCurrentDirectory(), archiveName);
            }

            if (File.Exists(_logFilePath))
            {
                File.Move(_logFilePath, archivePath, overwrite: true);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 📦 Log rotated ({reason}) -> {Path.GetFileName(archivePath)}");
                Console.ResetColor();
            }

            // Create new log file
            _logFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"watchdog-{_applicationName}-{DateTime.Now:yyyy-MM-dd}.log");
            _logFile = new StreamWriter(_logFilePath, append: true)
            {
                AutoFlush = true
            };
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Warning: Could not rotate log file: {ex.Message}");
            Console.ResetColor();
        }
    }

    private void LogToFile(string message)
    {
        try
        {
            // Check if rotation is needed before writing
            CheckAndRotateLogFile();

            if (_logFile != null)
            {
                _logFile.WriteLine(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not write to log file: {ex.Message}");
        }
    }

    // Cleanup resources
    public void Cleanup()
    {
        _logFile?.Flush();
        _logFile?.Dispose();
    }

    private static void PrintUsage()
    {
        Console.WriteLine("╔════════════════════════════════════════════════╗");
        Console.WriteLine("║       Watchdog Application - Usage Guide       ║");
        Console.WriteLine("╚════════════════════════════════════════════════╝\n");
        
        Console.WriteLine("Usage:");
        Console.WriteLine("  Watchdog.exe                                    (uses config.xml)");
        Console.WriteLine("  Watchdog.exe <executable_path> [arguments]     (direct mode)");
        Console.WriteLine("  Watchdog.exe <config_file.xml>                 (XML config mode)");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  Watchdog.exe                                    (loads from config.xml)");
        Console.WriteLine("  Watchdog.exe notepad.exe                        (monitor notepad)");
        Console.WriteLine("  Watchdog.exe C:\\MyApp\\app.exe --port 8080     (with arguments)");
        Console.WriteLine("  Watchdog.exe myapps.xml                        (load from custom XML)");
        Console.WriteLine();
        Console.WriteLine("XML Configuration Format:");
        Console.WriteLine("  <?xml version=\"1.0\" encoding=\"utf-8\"?>");
        Console.WriteLine("  <Applications>");
        Console.WriteLine("    <Application>");
        Console.WriteLine("      <Name>App Name</Name>");
        Console.WriteLine("      <Path>C:\\path\\to\\app.exe</Path>");
        Console.WriteLine("      <Arguments>--arg1 value1 --arg2</Arguments>");
        Console.WriteLine("    </Application>");
        Console.WriteLine("  </Applications>");
        Console.WriteLine();
        Console.WriteLine("Features:");
        Console.WriteLine("  • Monitor single or multiple applications");
        Console.WriteLine("  • Store configurations in XML");
        Console.WriteLine("  • Automatic restart on crash/exit");
        Console.WriteLine("  • Timestamped logging");
        Console.WriteLine("  • Graceful shutdown with Ctrl+C");
        Console.WriteLine();
    }
}
