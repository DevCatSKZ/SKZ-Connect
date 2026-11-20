#!/usr/bin/env python3
"""Eigenständiges Linux-Paket (SteamOS & Co.) als tar.gz packen.

Aufruf: py -3 linux/make-linux-tar.py <Out-Ordner> <Version> <Ordner mit skzconnect> <linux/steamos> <Repo-Wurzel>
Schreibt SKZ-Connect-Linux-<Version>-x86_64.tar.gz mit Ausführrechten für Programm und Skripte.
"""
import os
import io
import sys
import tarfile

def build_archive(out_dir, version, bin_dir, steam_dir, root):
    name = f"SKZ-Connect-Linux-{version}-x86_64.tar.gz"
    os.makedirs(out_dir, exist_ok=True)
    path = os.path.join(out_dir, name)
    files = [
        (os.path.join(bin_dir, "skzconnect"), "skzconnect", 0o755),
        (os.path.join(steam_dir, "install.sh"), "install.sh", 0o755),
        (os.path.join(steam_dir, "uninstall.sh"), "uninstall.sh", 0o755),
        (os.path.join(steam_dir, "skzconnect.service"), "skzconnect.service", 0o644),
        (os.path.join(root, "LICENSE"), "LICENSE.txt", 0o644),
        (os.path.join(root, "THIRD-PARTY-NOTICES.md"), "THIRD-PARTY-NOTICES.md", 0o644),
    ]

    with tarfile.open(path, "w:gz") as tar:
        for src, arc, mode in files:
            with open(src, "rb") as source:
                data = source.read()
            if arc != "skzconnect":
                # Linux-Skripte benötigen LF auch bei einem Windows-Checkout.
                data = data.removeprefix(b"\xef\xbb\xbf").replace(b"\r\n", b"\n").replace(b"\r", b"\n")
            info = tar.gettarinfo(src, arc)
            info.mode = mode
            info.size = len(data)
            info.uid = info.gid = 0
            info.uname = info.gname = ""
            with io.BytesIO(data) as payload:
                tar.addfile(info, payload)
    return path

if __name__ == "__main__":
    path = build_archive(*sys.argv[1:6])
    print(f"  {os.path.basename(path)} ({os.path.getsize(path) / 1048576:.1f} MB)")
