"""
Watchdog — core monitor loop that ties everything together.
"""

import time

from .config_parser import parse_config
from .constants import POLL_INTERVAL
from .logger import log
from .process_manager import ManagedProcess


class Watchdog:
    """Main watchdog loop: start, monitor, restart, shutdown."""

    def __init__(self, config_path: str):
        self.config_path = config_path
        self.managed: list[ManagedProcess] = []
        self._running = True

    def load_and_start(self):
        """Parse config and start all enabled applications."""
        apps = parse_config(self.config_path)
        log.info("Loaded %d application(s) from config", len(apps))

        for cfg in apps:
            if cfg.start:
                mp = ManagedProcess(cfg)
                if mp.start():
                    self.managed.append(mp)
            else:
                log.info("[%s] Start=false — skipped", cfg.name)

    def monitor(self):
        """Poll managed processes and restart any that have exited."""
        log.info(
            "Monitoring started (poll every %ds) — press Ctrl+C to stop",
            POLL_INTERVAL,
        )
        while self._running:
            for mp in self.managed:
                if not mp.is_alive():
                    code = mp.exit_code()
                    log.warning(
                        "[%s] Exited with code %s — restarting …",
                        mp.config.name,
                        code,
                    )
                    mp.start()
            try:
                time.sleep(POLL_INTERVAL)
            except (KeyboardInterrupt, SystemExit):
                self._running = False

    def shutdown(self):
        """Terminate all managed processes."""
        log.info("Shutting down — terminating managed processes …")
        for mp in self.managed:
            mp.terminate()
        log.info("All processes terminated. Goodbye!")

    def stop(self):
        """Signal the monitor loop to stop."""
        self._running = False
