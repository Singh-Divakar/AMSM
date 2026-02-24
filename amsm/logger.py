"""
Dual logging setup: console (INFO) + rotating file (DEBUG).
"""

import logging
import logging.handlers
import sys

from .constants import LOG_PATH, LOG_MAX_BYTES, LOG_BACKUP_COUNT

_FORMAT = "%(asctime)s | %(levelname)-8s | %(message)s"
_DATE_FORMAT = "%Y-%m-%d %H:%M:%S"


def setup_logging() -> logging.Logger:
    """Create and configure the 'watchdog' logger."""
    logger = logging.getLogger("watchdog")
    logger.setLevel(logging.DEBUG)

    fmt = logging.Formatter(_FORMAT, datefmt=_DATE_FORMAT)

    # Console handler
    console = logging.StreamHandler(sys.stdout)
    console.setLevel(logging.INFO)
    console.setFormatter(fmt)
    logger.addHandler(console)

    # Rotating file handler
    file_handler = logging.handlers.RotatingFileHandler(
        LOG_PATH,
        maxBytes=LOG_MAX_BYTES,
        backupCount=LOG_BACKUP_COUNT,
        encoding="utf-8",
    )
    file_handler.setLevel(logging.DEBUG)
    file_handler.setFormatter(fmt)
    logger.addHandler(file_handler)

    return logger


# Module-level logger instance — import this everywhere
log = setup_logging()
