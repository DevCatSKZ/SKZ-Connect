# -*- coding: utf-8 -*-
"""Kodi-Icons und Offline-Varianten aus den gemeinsamen Controllerbildern exportieren.

Auf dem Windows-Entwicklungsrechner: python linux/kodi/make_icons.py
Die fertigen PNGs werden eingecheckt; Kodi selbst braucht kein .NET zum Anzeigen.
"""
import pathlib
import subprocess


def main():
    root = pathlib.Path(__file__).resolve().parents[2]
    target = root / 'linux/kodi/service.skzconnect/resources/icons'
    subprocess.run([
        'dotnet', 'run', '--project', str(root / 'tools/ControllerArtPreview'),
        '-c', 'Release', '--artifacts-path', str(root / 'out/controller-icon-build'),
        '--', str(target), '--icons',
    ], cwd=root, check=True)


if __name__ == '__main__':
    main()
