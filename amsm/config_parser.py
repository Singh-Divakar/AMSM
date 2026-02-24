"""
Parse config.xml and produce AppConfig objects.
"""

import os
import sys
import xml.etree.ElementTree as ET

from .logger import log


class AppConfig:
    """Represents a single <Application> entry from config.xml."""

    def __init__(self, name: str, path: str, arguments: str, start: bool):
        self.name = name
        self.path = path
        self.arguments = arguments
        self.start = start

    def __repr__(self):
        return f"AppConfig(name={self.name!r}, start={self.start})"


def parse_config(config_path: str) -> list[AppConfig]:
    """Parse config.xml and return a list of AppConfig objects."""
    if not os.path.isfile(config_path):
        log.error("Config file not found: %s", config_path)
        sys.exit(1)

    tree = ET.parse(config_path)
    root = tree.getroot()
    apps: list[AppConfig] = []

    for app_el in root.findall("Application"):
        name = (app_el.findtext("Name") or "").strip()
        path = (app_el.findtext("Path") or "").strip()
        arguments = (app_el.findtext("Arguments") or "").strip()
        start_text = (app_el.findtext("Start") or "false").strip().lower()
        start = start_text == "true"
        apps.append(AppConfig(name, path, arguments, start))

    return apps
