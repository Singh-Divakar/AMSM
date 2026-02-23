#!/usr/bin/env python3
"""
AMSM Application Manager - Reads config.xml and starts applications with monitoring
"""

import os
import sys
import xml.etree.ElementTree as ET
import subprocess
import threading
import time
import psutil
from datetime import datetime, timedelta
from pathlib import Path

# Windows-specific imports for keyboard input
if os.name == 'nt':
    import msvcrt


class AMSMManager:
    """AMSM Application Manager class"""
    
    def __init__(self, config_path):
        self.config_path = config_path
        self.running_processes = []
        self.monitoring_active = True
        self.refresh_interval = 5  # seconds
    
    def read_config(self):
        """Read and parse the XML configuration file"""
        if not os.path.exists(self.config_path):
            print(f"❌ Config file not found: {self.config_path}")
            sys.exit(1)
        
        try:
            tree = ET.parse(self.config_path)
            root = tree.getroot()
            return root
        except ET.ParseError as e:
            print(f"❌ Failed to parse config file: {e}")
            sys.exit(1)
    
    def start_applications(self, root):
        """Start all applications defined in the configuration"""
        applications = root.findall('Application')
        
        if not applications:
            print("⚠️  No applications found in config")
            return []
        
        print(f"Found {len(applications)} application(s) in config\n")
        
        for app in applications:
            name = app.find('Name')
            path = app.find('Path')
            arguments = app.find('Arguments')
            
            name_text = name.text if name is not None else "Unknown"
            path_text = path.text if path is not None else None
            args_text = arguments.text if arguments is not None else ""
            
            if not path_text or not path_text.strip():
                print(f"⚠️  Skipping application '{name_text}' - no path specified")
                continue
            
            if not os.path.exists(path_text):
                print(f"⚠️  Skipping application '{name_text}' - executable not found: {path_text}")
                continue
            
            try:
                print(f"▶️  Starting application: {name_text}")
                print(f"    Path: {path_text}")
                
                if args_text and args_text.strip():
                    print(f"    Arguments: {args_text}")
                    process = subprocess.Popen(
                        f'"{path_text}" {args_text}',
                        shell=True,
                        stdout=subprocess.DEVNULL,
                        stderr=subprocess.DEVNULL
                    )
                else:
                    process = subprocess.Popen(
                        path_text,
                        stdout=subprocess.DEVNULL,
                        stderr=subprocess.DEVNULL
                    )
                
                self.running_processes.append({
                    'name': name_text,
                    'pid': process.pid,
                    'process': process,
                    'start_time': datetime.now(),
                    'path': path_text
                })
                
                print(f"    ✓ PID: {process.pid}\n")
                
            except Exception as e:
                print(f"❌ Failed to start application '{name_text}': {e}\n")
        
        return self.running_processes
    
    def get_process_info(self, pid):
        """Get process information by PID"""
        try:
            process = psutil.Process(pid)
            memory_mb = process.memory_info().rss / (1024 * 1024)
            return {
                'running': True,
                'memory': memory_mb,
                'process': process
            }
        except (psutil.NoSuchProcess, psutil.AccessDenied):
            return {'running': False}
    
    def format_uptime(self, start_time):
        """Format uptime as HH:MM:SS"""
        uptime = datetime.now() - start_time
        hours, remainder = divmod(uptime.total_seconds(), 3600)
        minutes, seconds = divmod(remainder, 60)
        return f"{int(hours):02d}:{int(minutes):02d}:{int(seconds):02d}"
    
    def clear_screen(self):
        """Clear the console screen"""
        sys.stdout.flush()
        os.system('cls' if os.name == 'nt' else 'clear')
    
    def monitor_applications(self):
        """Monitor running applications"""
        if not self.running_processes:
            print("⚠️  No applications to monitor")
            return
        
        print("\n" + "=" * 50)
        print("Monitoring Applications")
        print("=" * 50 + "\n")
        sys.stdout.flush()
        
        still_running = []
        
        while self.monitoring_active:
            # Clear screen removed for debugging
            print("\n" + "=" * 50)
            print(f"AMSM Application Monitor - {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
            print("=" * 50 + "\n")
            
            still_running = []
            
            for app_info in self.running_processes:
                name = app_info['name']
                pid = app_info['pid']
                start_time = app_info['start_time']
                
                info = self.get_process_info(pid)
                
                if not info['running']:
                    print(f"[{name}] - TERMINATED 🔴")
                else:
                    uptime = self.format_uptime(start_time)
                    memory = info['memory']
                    print(f"[{name}] - RUNNING 🟢")
                    print(f"  PID: {pid}, Memory: {memory:.2f} MB, Uptime: {uptime}")
                    still_running.append(app_info)
            
            print("\nPress 'Q' to quit monitoring, or wait for next update...")
            print(f"Updating in {self.refresh_interval} seconds...\n")
            sys.stdout.flush()
            
            # Wait with keyboard interrupt (Windows compatible)
            try:
                for i in range(self.refresh_interval):
                    if os.name == 'nt' and msvcrt.kbhit():
                        key = msvcrt.getwche().upper()
                        if key == 'Q':
                            self.monitoring_active = False
                            break
                    time.sleep(1)
            except KeyboardInterrupt:
                self.monitoring_active = False
                print("\n⏸️  Monitoring interrupted")
            
            if len(still_running) == 0:
                print("\nAll applications have terminated. 🛑\n")
                break
        
        self.monitoring_active = False
    
    def run(self):
        """Main execution method"""
        print("╔════════════════════════════════════════╗")
        print("║   AMSM Application Manager             ║")
        print("╚════════════════════════════════════════╝\n")
        
        # Read configuration
        print(f"Reading configuration from: {self.config_path}")
        config = self.read_config()
        
        # Check if config has data
        if not config.findall('Application'):
            print("❌ No applications configured in config.xml")
            sys.exit(1)
        
        # Start applications
        processes = self.start_applications(config)
        
        if processes:
            print(f"✓ {len(processes)} application(s) started successfully\n")
            
            # Monitor applications
            self.monitor_applications()
        else:
            print("❌ No applications were started")
            sys.exit(1)
        
        print("\nAMSM Manager shutdown complete. 👋\n")


def main():
    """Main entry point"""
    config_path = r"f:\AMSM\config.xml"
    
    # Allow custom config path as command line argument
    if len(sys.argv) > 1:
        config_path = sys.argv[1]
    
    manager = AMSMManager(config_path)
    manager.run()


if __name__ == "__main__":
    main()
