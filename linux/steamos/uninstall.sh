#!/usr/bin/env bash
# SKZ Connect entfernen: sudo ./uninstall.sh [--all]  (mit --all auch Einstellungen und Protokolle)
set -euo pipefail
if [ "$(id -u)" -ne 0 ]; then
    echo "Bitte mit sudo ausführen." >&2
    exit 1
fi
systemctl disable --now skzconnect 2>/dev/null || true
rm -f /etc/systemd/system/skzconnect.service
rm -f /etc/modules-load.d/skzconnect-uinput.conf
rm -f /var/lib/skzconnect/skzconnect
if [ "${1:-}" = "--all" ]; then
    rm -rf /var/lib/skzconnect
fi
systemctl daemon-reload
echo "SKZ Connect entfernt."
