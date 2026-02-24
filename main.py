"""
AMSM Watchdog — Entry Point
Run:  python main.py
"""

import signal

from amsm.constants import CONFIG_PATH, LOG_PATH
from amsm.logger import log
from amsm.watchdog import Watchdog


def main():
    log.info("=" * 60)
    log.info("AMSM Watchdog starting")
    log.info("Config : %s", CONFIG_PATH)
    log.info("Log    : %s", LOG_PATH)
    log.info("=" * 60)

    wd = Watchdog(CONFIG_PATH)

    # Handle Ctrl+C / SIGTERM
    def _signal_handler(signum, frame):
        log.info("Received signal %s", signum)
        wd.stop()

    signal.signal(signal.SIGINT, _signal_handler)
    signal.signal(signal.SIGTERM, _signal_handler)

    try:
        wd.load_and_start()
        wd.monitor()
    finally:
        wd.shutdown()


if __name__ == "__main__":
    main()
