#!/usr/bin/env python3
"""Eigenständiges Linux-Paket (SteamOS & Co.) als tar.gz packen.

Aufruf: py -3 linux/make-linux-tar.py <Out-Ordner> <Version> <Ordner mit skzconnect> <linux/steamos> <Repo-Wurzel>
Schreibt SKZ-Connect-Linux-<Version>-x86_64.tar.gz mit Ausführrechten für Programm und Skripte.
"""
import os
import sys
import tarfile

out_dir, version, bin_dir, steam_dir, root = sys.argv[1:6]
name = f"SKZ-Connect-Linux-{version}-x86_64.tar.gz"
path = os.path.join(out_dir, name)

FILES = [
    (os.path.join(bin_dir, "skzconnect"), "skzconnect", 0o755),
    (os.path.join(steam_dir, "install.sh"), "install.sh", 0o755),
    (os.path.join(steam_dir, "uninstall.sh"), "uninstall.sh", 0o755),
    (os.path.join(steam_dir, "skzconnect.service"), "skzconnect.service", 0o644),
    (os.path.join(root, "LICENSE"), "LICENSE.txt", 0o644),
    (os.path.join(root, "THIRD-PARTY-NOTICES.md"), "THIRD-PARTY-NOTICES.md", 0o644),
]

with tarfile.open(path, "w:gz") as tar:
    for src, arc, mode in FILES:
        info = tar.gettarinfo(src, arc)
        info.mode = mode
        with open(src, "rb") as f:
            tar.addfile(info, f)

print(f"  {name} ({os.path.getsize(path) / 1048576:.1f} MB)")
