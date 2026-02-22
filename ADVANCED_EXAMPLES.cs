// Advanced Configuration Examples
// These are code snippets showing how to customize the watchdog for different use cases

/*
 * EXAMPLE 1: Capture Application Output
 * Modify the StartApplication method to capture stdout/stderr
 */

// Replace the processInfo setup with:
/*
var processInfo = new ProcessStartInfo
{
    FileName = executablePath,
    Arguments = arguments,
    UseShellExecute = false,
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    CreateNoWindow = true  // Don't show separate window
};

Process process = Process.Start(processInfo)!;

// Read output asynchronously
_ = Task.Run(() =>
{
    string line;
    while ((line = process.StandardOutput.ReadLine()) != null)
    {
        Console.WriteLine($"[APP] {line}");
    }
});

_ = Task.Run(() =>
{
    string line;
    while ((line = process.StandardError.ReadLine()) != null)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[APP-ERR] {line}");
        Console.ResetColor();
    }
});
*/

/*
 * EXAMPLE 2: Custom Restart Delay Based on Exit Code
 * Restart faster for normal exits, slower for crashes
 */

/*
if (exitCode == 0)
{
    // Normal exit - restart immediately or wait less
    await Task.Delay(500);
}
else
{
    // Crash or error - wait longer
    await Task.Delay(5000);
}
*/

/*
 * EXAMPLE 3: Maximum Restart Attempts
 * Stop restarting if it fails too many times
 */

/*
private int _maxRestarts = 10;

// In WatchApplicationAsync:
if (_restartCount >= _maxRestarts)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[ERROR] Maximum restart attempts ({_maxRestarts}) reached. Stopping.");
    Console.ResetColor();
    _isRunning = false;
    break;
}
*/

/*
 * EXAMPLE 4: Environment Variables
 * Pass custom environment variables to the child process
 */

/*
var processInfo = new ProcessStartInfo
{
    FileName = executablePath,
    Arguments = arguments,
    UseShellExecute = false
};

// Add custom environment variables
processInfo.EnvironmentVariables.Add("WATCHDOG_ENABLED", "true");
processInfo.EnvironmentVariables.Add("RESTART_COUNT", _restartCount.ToString());
processInfo.EnvironmentVariables.Add("PARENT_PID", Process.GetCurrentProcess().Id.ToString());

Process process = Process.Start(processInfo)!;
*/

/*
 * EXAMPLE 5: File Logging
 * Log watchdog events to a file instead of console only
 */

/*
private StreamWriter? _logFile;

public WatchdogApp(string logPath)
{
    _logFile = new StreamWriter(logPath, append: true)
    {
        AutoFlush = true
    };
}

private void Log(string message, ConsoleColor color = ConsoleColor.White)
{
    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    string logMessage = $"[{timestamp}] {message}";
    
    Console.ForegroundColor = color;
    Console.WriteLine(logMessage);
    Console.ResetColor();
    
    _logFile?.WriteLine(logMessage);
}
*/

/*
 * EXAMPLE 6: Health Check / Heartbeat
 * Monitor process health and restart if it stops responding
 */

/*
private async Task MonitorHealthAsync(Process process, int healthCheckIntervalMs = 5000)
{
    while (!process.HasExited)
    {
        await Task.Delay(healthCheckIntervalMs);
        
        if (process.HasExited)
            break;
            
        // Check if process is still responsive
        // You could send a health check request to the app
        // via HTTP, named pipes, or other IPC
        
        try
        {
            if (process.Responding)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 💓 Health check passed (PID: {process.Id})");
            }
        }
        catch
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ⚠️  Health check failed - restarting");
            process.Kill();
        }
    }
}
*/

/*
 * EXAMPLE 7: Configuration File Support
 * Load watchdog settings from JSON config
 */

/*
using System.Text.Json;

public class WatchdogConfig
{
    public string ExecutablePath { get; set; }
    public string Arguments { get; set; }
    public int RestartDelayMs { get; set; } = 2000;
    public int MaxRestarts { get; set; } = -1; // -1 = unlimited
    public bool CaptureOutput { get; set; } = false;
    public string LogFilePath { get; set; } = "";
}

private static WatchdogConfig LoadConfig(string configPath)
{
    var json = File.ReadAllText(configPath);
    return JsonSerializer.Deserialize<WatchdogConfig>(json)!;
}

// Usage:
// watchdog-config.json:
/*
{
  "executablePath": "C:\\MyApp\\app.exe",
  "arguments": "--port 8080",
  "restartDelayMs": 3000,
  "maxRestarts": 5,
  "captureOutput": true,
  "logFilePath": "watchdog.log"
}
*/

/*
 * EXAMPLE 8: Process Resource Limits
 * Set memory/CPU limits for the watched application
 */

/*
using System.ComponentModel;

private void SetProcessPriority(Process process, ProcessPriorityClass priority)
{
    try
    {
        process.PriorityClass = priority;
        Console.WriteLine($"Process priority set to: {priority}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Could not set priority: {ex.Message}");
    }
}

// Usage:
// SetProcessPriority(process, ProcessPriorityClass.BelowNormal);
*/

/*
 * EXAMPLE 9: Restart Statistics
 * Track detailed restart statistics
 */

/*
public class RestartStatistics
{
    public int TotalRestarts { get; set; }
    public Dictionary<int, int> ExitCodeCounts { get; set; } = new();
    public DateTime FirstRestartTime { get; set; }
    public DateTime LastRestartTime { get; set; }
    public TimeSpan UpTime { get; set; }
    
    public void RecordRestart(int exitCode)
    {
        TotalRestarts++;
        if (!ExitCodeCounts.ContainsKey(exitCode))
            ExitCodeCounts[exitCode] = 0;
        ExitCodeCounts[exitCode]++;
        LastRestartTime = DateTime.Now;
    }
    
    public void PrintReport()
    {
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║       Restart Statistics Report        ║");
        Console.WriteLine("╚════════════════════════════════════════╝");
        Console.WriteLine($"Total Restarts: {TotalRestarts}");
        Console.WriteLine($"First Restart: {FirstRestartTime}");
        Console.WriteLine($"Last Restart: {LastRestartTime}");
        Console.WriteLine($"Total Uptime: {UpTime}");
        Console.WriteLine("\nExit Code Distribution:");
        foreach (var kvp in ExitCodeCounts.OrderByDescending(x => x.Value))
        {
            Console.WriteLine($"  Code {kvp.Key}: {kvp.Value} times");
        }
    }
}
*/

/*
 * EXAMPLE 10: Notification/Alert System
 * Send alerts when application crashes repeatedly
 */

/*
private async Task NotifyIfTooManyRestarts(int restartCount, int threshold = 5)
{
    if (restartCount > threshold)
    {
        // Send email alert
        // SendEmailAlert($"Application restarted {restartCount} times!");
        
        // Send webhook notification
        // await SendWebhookAlert($"Watchdog Alert: Too many restarts ({restartCount})");
        
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"🚨 ALERT: Application restarted {restartCount} times in a short period!");
        Console.ResetColor();
    }
}
*/

// COMMON CUSTOMIZATIONS CHECKLIST
// ================================
// [ ] Add file logging
// [ ] Capture child process output
// [ ] Set custom restart delays
// [ ] Implement max restart limits
// [ ] Add health monitoring
// [ ] Load configuration from file
// [ ] Send email/webhook alerts on repeated crashes
// [ ] Track detailed statistics
// [ ] Set process resource limits
// [ ] Customize console output format
// [ ] Add support for multiple processes
// [ ] Implement graceful shutdown sequences
