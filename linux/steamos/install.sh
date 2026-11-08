#!/usr/bin/env bash
# SKZ Connect auf SteamOS (oder jedem Linux mit systemd + BlueZ) einrichten.
# Liegt das Programm „skzconnect“ neben diesem Skript, wird es installiert;
# andernfalls lädt das Skript das Paket von GitHub herunter:
#   sudo ./install.sh                – Paket neben dem Skript verwenden oder herunterladen (neueste Version)
#   sudo ./install.sh --version 0.4.11 – bestimmte Version herunterladen
set -euo pipefail

DIR=/var/lib/skzconnect
UNIT=skzconnect.service
REPO=DevCatSKZ/SKZ-Connect
VERSION=""

while [ $# -gt 0 ]; do
    case "$1" in
        --version) VERSION="${2:?--version braucht einen Wert}"; shift 2 ;;
        -h|--help) sed -n '2,7p' "$0"; exit 0 ;;
        *) echo "Unbekannte Option: $1" >&2; exit 1 ;;
    esac
done

if [ "$(id -u)" -ne 0 ]; then
    echo "Bitte mit sudo ausführen: sudo $0" >&2
    exit 1
fi
if ! command -v systemctl >/dev/null; then
    echo "systemd wird benötigt." >&2
    exit 1
fi

# --- Programm beschaffen -----------------------------------------------------
SELF_DIR="$(cd "$(dirname "$0")" && pwd)"
TMP=""
if [ ! -f "$SELF_DIR/skzconnect" ]; then
    command -v curl >/dev/null || { echo "curl fehlt und keine Datei 'skzconnect' liegt hier." >&2; exit 1; }
    # Die Download-Adresse des passenden Pakets aus den Release-Dateien lesen (neuestes Release zuerst).
    URL=$(curl -fsSL "https://api.github.com/repos/$REPO/releases" \
        | grep -o "\"https://github.com/$REPO/releases/download/[^\"]*SKZ-Connect-Linux-[0-9.]*-x86_64.tar.gz\"" \
        | tr -d '"' \
        | { [ -z "$VERSION" ] && head -1 || grep "SKZ-Connect-Linux-$VERSION-x86_64" || true; })
    [ -n "$URL" ] || { echo "Kein Linux-Paket am Release gefunden – das Skript zusammen mit dem Paket entpacken." >&2; exit 1; }
    TMP=$(mktemp -d)
    echo "Lade $(basename "$URL") …"
    curl -fsSL "$URL" -o "$TMP/pkg.tar.gz"
    tar -xzf "$TMP/pkg.tar.gz" -C "$TMP"
    SELF_DIR="$TMP"
fi
[ -f "$SELF_DIR/skzconnect" ] || { echo "Datei 'skzconnect' fehlt." >&2; exit 1; }

# --- Voraussetzungen ---------------------------------------------------------
systemctl enable --now bluetooth 2>/dev/null || true
if [ ! -e /dev/uinput ]; then
    modprobe uinput 2>/dev/null || true
    echo uinput > /etc/modules-load.d/skzconnect-uinput.conf
fi
if [ ! -e /dev/uinput ]; then
    echo "Warnung: /dev/uinput fehlt – der Kernel braucht das Modul 'uinput'. Virtuelle Controller gehen sonst nicht." >&2
fi

# --- Installieren ------------------------------------------------------------
systemctl stop "$UNIT" 2>/dev/null || true
mkdir -p "$DIR"
install -m 755 "$SELF_DIR/skzconnect" "$DIR/skzconnect"
install -m 644 "$SELF_DIR/$UNIT" /etc/systemd/system/$UNIT
systemctl daemon-reload
systemctl enable --now "$UNIT"

rm -rf "${TMP:-/nonexistent}" 2>/dev/null || true

cat <<EOF

Fertig. SKZ Connect läuft jetzt als Dienst 'skzconnect'.

  Koppeln:   SYNC-Taste am Controller drücken (klein, an der Oberkante) –
             danach verbindet ein Tastendruck wieder.
  Status:    cat $DIR/status.json
  Protokoll: $DIR/skzconnect.log
  Selbsttest: sudo $DIR/skzconnect --selftest --data $DIR
  Stoppen:   sudo systemctl stop skzconnect
  Update:    Skript erneut mit dem neuen Paket ausführen
EOF
