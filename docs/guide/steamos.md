# SKZ Connect on SteamOS and Linux

[Deutsch](../anleitung/steamos.md) · **English** · As of version 0.4.11

SKZ Connect connects Nintendo Switch 2 controllers (Pro Controller 2, Joy-Con 2, GameCube controller) over Bluetooth
to SteamOS and passes them on to Steam, games and emulators as virtual Xbox 360 controllers. The service runs in the
background – including Game Mode – and needs no UI: pairing is done with the SYNC button on the controller. The same
service runs on any Linux distribution with systemd and BlueZ.

## Requirements

- SteamOS 3.x (including PC/HTPC installs) or Linux with systemd and BlueZ, 64-bit (x86_64).
- Bluetooth adapter with Bluetooth LE (4.0 or later); internal or USB dongle.
- **Nothing changes on the read-only system:** program and service live in `/var/lib/skzconnect` and
  `/etc/systemd/system` – `steamos-readonly` stays enabled.

## Setup

1. Download `SKZ-Connect-Linux-<version>-x86_64.tar.gz` from the
   [release](https://github.com/DevCatSKZ/SKZ-Connect/releases) and unpack it – or just save `install.sh`, which
   downloads the package itself.
2. Run `sudo ./install.sh`. The script installs the binary and a systemd unit, enables Bluetooth and starts the
   `skzconnect` service. Running it again updates to the new version.

## Pairing

Briefly press the SYNC button on the controller (small, on the top edge or in the battery compartment). The
controller shows up in Steam as an Xbox 360 controller. Afterwards a single button press reconnects – even after a
reboot. For Joy-Con 2, pair one controller per side; SKZ Connect combines pairs automatically.

## Checking and troubleshooting

- `cat /var/lib/skzconnect/status.json` – live state (Bluetooth, connected controllers, battery).
- `sudo /var/lib/skzconnect/skzconnect --selftest --data /var/lib/skzconnect` – diagnostics without a controller.
- `sudo /var/lib/skzconnect/skzconnect --test-pad 30 --data /var/lib/skzconnect` – creates a test controller for
  30 seconds: if Steam sees it, the problem is Bluetooth, otherwise the output.
- Log: `/var/lib/skzconnect/skzconnect.log`.

## Removing

`sudo ./uninstall.sh` (add `--all` to delete settings and logs too).

## Notes

- Classic controllers (Switch 1, Wii, PlayStation, Xbox) are usually handled by the Linux kernel drivers and Steam
  Input already – SKZ Connect is for the Switch 2 controllers.
- The service does not register a default pairing agent: pairing other devices (keyboard, headset) through the
  system is unaffected.
- Settings (remapping, gyro etc.) live in `/var/lib/skzconnect/settings.json` in the same format as on Windows; the
  service picks up changes while running.
