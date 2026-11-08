# SKZ Connect unter SteamOS und Linux

**Deutsch** · [English](../guide/steamos.md) · Stand: Version 0.4.11

SKZ Connect verbindet Nintendo-Switch-2-Controller (Pro Controller 2, Joy-Con 2, GameCube-Controller) über Bluetooth
mit SteamOS und gibt sie als virtuelle Xbox-360-Controller an Steam, Spiele und Emulatoren weiter. Der Dienst läuft
im Hintergrund – auch im Game Mode – und braucht keine Oberfläche: gekoppelt wird über die SYNC-Taste am Controller.
Auf anderen Linux-Distributionen mit systemd und BlueZ läuft derselbe Dienst genauso.

## Voraussetzungen

- SteamOS 3.x (auch auf PC/HTPC) oder Linux mit systemd und BlueZ, 64 Bit (x86_64).
- Bluetooth-Adapter mit Bluetooth LE (ab 4.0); interner oder USB-Dongle.
- **Nichts am schreibgeschützten System ändern:** Installation und Dienst liegen in `/var/lib/skzconnect` und
  `/etc/systemd/system` – `steamos-readonly` bleibt eingeschaltet.

## Einrichten

1. Das Paket `SKZ-Connect-Linux-<Version>-x86_64.tar.gz` vom
   [Release](https://github.com/DevCatSKZ/SKZ-Connect/releases) laden und entpacken – oder nur `install.sh`
   speichern, dann lädt das Skript das Paket selbst.
2. `sudo ./install.sh` ausführen. Das Skript legt Programm und systemd-Einheit an, schaltet Bluetooth ein und
   startet den Dienst `skzconnect`. Erneut ausführen = Update auf die neue Version.

## Koppeln

Die SYNC-Taste am Controller kurz drücken (klein, an der Oberkante bzw. im Batteriefach). Der Controller erscheint in
Steam als Xbox-360-Controller. Danach reicht ein Tastendruck zum Wiederverbinden – auch nach einem Neustart.
Bei Joy-Con 2 einen Controller je Seite koppeln; SKZ Connect fasst Paare automatisch zusammen.

## Prüfen und Fehlersuche

- `cat /var/lib/skzconnect/status.json` – laufender Zustand (Bluetooth, verbundene Controller, Akku).
- `sudo /var/lib/skzconnect/skzconnect --selftest --data /var/lib/skzconnect` – Diagnose ohne Controller.
- `sudo /var/lib/skzconnect/skzconnect --test-pad 30 --data /var/lib/skzconnect` – legt 30 Sekunden einen
  Test-Controller an: sieht Steam ihn, liegt das Problem am Bluetooth, sonst an der Ausgabe.
- Protokoll: `/var/lib/skzconnect/skzconnect.log`.

## Entfernen

`sudo ./uninstall.sh` (mit `--all` auch Einstellungen und Protokolle löschen).

## Hinweise

- Die klassischen Controller (Switch 1, Wii, PlayStation, Xbox) übernehmen auf dem Desktop meist schon die
  Linux-Kerneltreiber und Steam Input – SKZ Connect ist für die Switch-2-Controller da.
- Der Dienst registriert keinen Standard-Kopplungs-Agenten: Das Koppeln anderer Geräte (Tastatur, Headset) über das
  System bleibt unberührt.
- Einstellungen (Belegung, Gyro usw.) liegen in `/var/lib/skzconnect/settings.json` im gleichen Format wie bei
  Windows; der Dienst übernimmt Änderungen laufend.
