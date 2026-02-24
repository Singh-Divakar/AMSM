"""
ManagedProcess — wraps subprocess.Popen for monitored applications.
"""

import os
import subprocess
import sys

from .config_parser import AppConfig
from .logger import log


class ManagedProcess:
    """Wraps a subprocess.Popen with metadata for monitoring."""

    def __init__(self, config: AppConfig):
        self.config = config
        self.process: subprocess.Popen | None = None

    def start(self) -> bool:
        """Launch the process. Returns True on success."""
        if not os.path.isfile(self.config.path):
            log.warning(
                "[%s] Executable not found: %s — skipping",
                self.config.name,
                self.config.path,
            )
            return False

        cmd = [self.config.path]
        if self.config.arguments:
            cmd.extend(self.config.arguments.split())

        try:
            self.process = subprocess.Popen(
                cmd,
                stdout=subprocess.DEVNULL,
                stderr=subprocess.DEVNULL,
                creationflags=(
                    subprocess.CREATE_NEW_PROCESS_GROUP
                    if sys.platform == "win32"
                    else 0
                ),
            )
            log.info(
                "[%s] Started  (PID %d) — %s",
                self.config.name,
                self.process.pid,
                " ".join(cmd),
            )
            return True
        except Exception as exc:
            log.error("[%s] Failed to start: %s", self.config.name, exc)
            return False

    def is_alive(self) -> bool:
        """Return True if the process is still running."""
        if self.process is None:
            return False
        return self.process.poll() is None

    def exit_code(self) -> int | None:
        """Return the exit code, or None if still running."""
        if self.process is None:
            return None
        return self.process.poll()

    def terminate(self):
        """Gracefully terminate the process."""
        if self.process and self.is_alive():
            try:
                self.process.terminate()
                self.process.wait(timeout=5)
                log.info(
                    "[%s] Terminated (PID %d)",
                    self.config.name,
                    self.process.pid,
                )
            except subprocess.TimeoutExpired:
                self.process.kill()
                log.warning(
                    "[%s] Killed (PID %d)",
                    self.config.name,
                    self.process.pid,
                )
            except Exception as exc:
                log.error("[%s] Error terminating: %s", self.config.name, exc)
