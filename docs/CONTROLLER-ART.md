# Controllerdarstellungen

Stand: 02.11.2026. Die Windows-Buttonanzeige verwendet 28 eingebettete PNGs
unter `src/Switch2Pro.Bridge/Assets/Controllers/`. Einheitliche Frontansicht,
transparenter Hintergrund, plastische Gehäuse und lesbare Bedienelemente.

- Switch 1 und Switch 2 Pro Controller; unbekannte Pads verwenden Pro 1 als neutrale Darstellung.
- GameCube (Switch 2), Xbox, DualShock 4 und DualSense.
- NES, SNES, Nintendo 64, Mega Drive und Wii U Pro.
- Wii Remote, MotionPlus, Nunchuk, MotionPlus mit Nunchuk und Classic Controller.
- Joy-Con 1 und 2: links/rechts, jeweils hochkant und quer, als Paar und im Grip.

`InputView.Artwork.cs` wählt das Bild. Die vorhandenen Vektorkoordinaten zeichnen
darüber ausschließlich Live-Eingaben: Tasten, Richtungstasten, bewegte Sticks,
analoge Trigger, Spieler-LEDs, Lichtleiste, Gyro, Mausmarkierung und Wii-Zeiger.
Das Gehäusebild hat feste Modellfarben; bewegte Stickkappen verwenden dazu
passende Materialfarben. Gerätefarben bleiben im Vektorrückfall erhalten.
Ohne Bildressource bleibt die bisherige
Vektordarstellung als Entwicklungsrückfall erhalten.

## Darstellung umschalten

Oben rechts auf der Controller-Seite schaltet **Controllerbilder / Nur Tasten**
zwischen Produktbild und reinen Live-Tastenfeldern um. Die Auswahl gilt für alle
Karten, bleibt nach einem Neustart erhalten und funktioniert in großer und
kompakter Ansicht. Vorhandene Einstellungen behalten die Bildansicht als Standard.
Analoge Trigger zeigen ihren Prozentwert; beim N64 werden die vier C-Tasten aus
den entsprechenden Achsen abgebildet. Joy-Con-Ausrichtung, Generation und Grip
sowie Wii-Erweiterungen bestimmen die angebotenen Tastenfelder.

Die GameCube-Schultermarkierungen verwenden separat am Bild abgenommene
L-/R-Konturen. Füllung und Leuchtrand sind auf die tatsächliche Kappenfläche
begrenzt. Einzel-, Analog- und gleichzeitige Eingaben werden mit
`--gamecube` in der fensterfreien Prüfhilfe dargestellt.

GUI-Prüfung mit `--buttons-only`: dunkel/normal und hell/kompakt bei simulierten
150 %. Der sichtbare Umschalter wurde in beiden Richtungen geprüft; die
Prüfhilfe stellt anschließend die Ausgangsansicht wieder her. Die Übersetzungen
der beiden Auswahltexte sind in allen zwölf UI-Sprachen vorhanden.

Die Bilder wurden mit dem eingebauten Imagegen-Werkzeug anhand der bisherigen
Layoutansichten erstellt. Die vollständigen Prompts stehen in
`controller-art-prompts.json`.

Fensterfreie Bildprüfung (keine Hardwareverbindung, kein Hauptfenster):

```powershell
dotnet run --project tools/ControllerArtPreview -c Release --artifacts-path out/controller-art-build -- out/controller-art-after
```

Die Prüfhilfe speichert Ruhe- und Eingabeansichten einschließlich aller
Joy-Con-Ausrichtungen. Tests prüfen die Bildabdeckung, Format/Transparenz und
die Rückkehr zum Ruhezustand nach dem Loslassen von Tasten.

## Gemeinsame Themes und Spielernummern

Alle 14 Farbschemata verwenden die Paletten aus SKZ Monitor und SKZ Remote.
Glasflächen, Buttonfüllungen, Rahmen und Textkontraste werden für Windows und
Android gemeinsam in `ControlAppearance` berechnet. Nova verwendet sechs
Verlaufsfarben, die Prismenschemata drei. Die Spielernummern erhalten je
Farbschema unterscheidbare Kreisfarben mit kontrastreicher Schrift; das gilt
auch für die Windows-Verbindungseinblendungen.

Die Windows-Prüfhilfe `--themes` exportiert alle 28 Kombinationen aus Schema und
Hell-/Dunkelmodus. Zusätzlich wurden die echte Übersicht, beide Bildmodi und
die Verbindungseinblendungen in der Windows-GUI geprüft.

## Android

Die Android-App bindet dieselben 28 Quelldateien als Assets ein. Die
Controllerkarten zeigen das passende Modellbild und darunter die vorhandene
Live-Eingabeanzeige. Es handelt sich dort um ein Modellbild ohne die bewegten
Windows-Bildüberlagerungen. Bekannte, getrennte Controller erscheinen entsättigt.
In den Einstellungen lassen sich die Controllerbilder ausblenden; die
Live-Eingaben bleiben sichtbar. Diese Anzeigepräferenz bleibt auch beim
Zurücksetzen der Controllerbelegung erhalten.

Die Bilder erweitern nicht die unterstützten Bluetooth-Gerätetypen. Die
Release-APK enthält arm64-v8a und x86_64. Native Debug- und Release-Builds sowie
die Übersetzungsprüfung wurden ausgeführt; ein Android-Gerätetest steht aus.

## Kodi / LibreELEC

`ControllerArtwork` ist der gemeinsame Modellkatalog. Der Exporter
`ControllerIconExporter` erzeugt daraus 128×128-PNGs mit transparentem Rand,
jeweils farbig und entsättigt für getrennte Geräte. Zusammen mit sechs
Kompatibilitätsnamen entstehen 68 Icons. Generationen, Joy-Con-Ausrichtungen,
Paar/Grip sowie PlayStation-Modelle werden getrennt zugeordnet.

`linux/kodi/make_icons.py` startet den Export aus den Windows-Quelldateien;
auf dem Kodi-Gerät ist dieser Entwicklungsschritt nicht erforderlich. Alle
LibreELEC-Pakete (x86_64, ARM64, ARM und Universalpaket) enthalten den gleichen
Iconbestand. Die Kodi-Oberfläche verwendet weiterhin den gewählten Kodi-Skin.
Paketintegrität und Python-Tests wurden geprüft; ein Kodi-Gerätetest steht aus.
