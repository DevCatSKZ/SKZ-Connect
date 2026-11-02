# Bauen und testen / Build and test

Requirements: .NET 10 SDK and Python 3. Windows builds require Windows.
Android additionally requires the .NET Android workload, JDK 21 and Android SDK.
Set `JAVA_HOME` and `ANDROID_HOME` to your own installations.

```powershell
dotnet build Switch2Pro.sln -c Release
dotnet test Switch2Pro.sln -c Release
python -m unittest discover -s linux/kodi/tests
python android/i18n/generate.py --check
dotnet workload install android
dotnet build android/SkzConnect.Android -c Debug "-p:JavaSdkDirectory=$env:JAVA_HOME" "-p:AndroidSdkDirectory=$env:ANDROID_HOME"
```

Windows portable build:

```powershell
dotnet publish src/Switch2Pro.Bridge -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=none -o out/publish/win-x64
```

The Inno Setup script is `installer/SKZ-Connect.iss`. It expects the published
Windows application in `out/publish/win-x64` and the redistributable installers
in `installer/redist`. The Windows workflow documents their verified sources.

Kodi packages: `powershell -File linux/build-kodi-addon.ps1`. Android signing
instructions: [ANDROID.md](ANDROID.md). Never commit signing keys, credentials,
real controller diagnostics or local device identifiers.

ViGEmBus 1.22.0 is the final release of the archived Windows virtual-controller
driver. It is required for virtual Xbox 360 / DualShock 4 output. Android and
Linux use different output backends. Third-party notices:
[THIRD-PARTY-NOTICES.md](../THIRD-PARTY-NOTICES.md).
