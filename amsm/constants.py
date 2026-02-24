"""
Shared constants for the AMSM watchdog.
"""

import os

# Base directory is the project root (parent of this package)
BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

# Paths
CONFIG_PATH = os.path.join(BASE_DIR, "config.xml")
LOG_PATH = os.path.join(BASE_DIR, "watchdog.log")

# Monitoring
POLL_INTERVAL = 5  # seconds between health checks

# Logging
LOG_MAX_BYTES = 5 * 1024 * 1024  # 5 MB per log file
LOG_BACKUP_COUNT = 3             # number of rotated backups
