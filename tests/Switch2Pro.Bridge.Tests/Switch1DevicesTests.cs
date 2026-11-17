using Switch2Pro.Protocol;

namespace Switch2Pro.Bridge.Tests;

/// <summary>
/// Erkennung von Switch-1-Controllern am HID-Pfad – inklusive lizenzierter Nachbauten
/// (PowerA „Enhanced Wireless“ u. a.), die über Bluetooth VID/PID 0000:0000 melden und
/// nur an ihrer Produktzeichenkette („Lic Pro Controller“) zu erkennen sind.
/// </summary>
public sealed class Switch1DevicesTests
{
    private const string ProBluetooth =
        @"\\?\hid#{00001124-0000-1000-8000-00805f9b34fb}_vid&0002057e_pid&2009#9&1&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}";
    private const string ProUsb =
        @"\\?\hid#vid_057e&pid_2009#9&1&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}";
    // PowerA „Enhanced Wireless“ (und andere Lizenz-Controller): Bluetooth-HID mit VID/PID 0000:0000.
    // Die ersten vier Zeichen hinter „vid&“ sind die VID-Herkunft (0000/0001/0002), die VID steht dahinter.
    private const string LicensedBluetooth =
        @"\\?\hid#{00001124-0000-1000-8000-00805f9b34fb}_vid&00000000_pid&0000#9&1&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}";

    [Fact]
    public void NintendoController_AmVidErkannt()
    {
        Assert.Equal(ControllerKind.Pro1, Switch1Devices.KindFromHidPath(ProBluetooth));
        Assert.Equal(ControllerKind.Pro1, Switch1Devices.KindFromHidPath(ProUsb));
        Assert.Equal(ControllerKind.JoyCon1Left,
            Switch1Devices.KindFromHidPath(ProBluetooth.Replace("2009", "2006")));
    }

    [Fact]
    public void LizenzController_NichtAmVidErkannt()
    {
        // VID 0000 trifft keine Nintendo-Kennung – die Erkennung läuft über die Produktzeichenkette.
        Assert.Equal(ControllerKind.Unknown, Switch1Devices.KindFromHidPath(LicensedBluetooth));
    }

    [Theory]
    [InlineData(LicensedBluetooth, true)]
    [InlineData(ProBluetooth, false)]
    [InlineData(ProUsb, false)]
    [InlineData(@"\\?\hid#vid_20d6&pid_a711#9&1&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}", false)]
    public void NullVid_NurBeiBluetoothLizenzpfad(string path, bool expected) =>
        Assert.Equal(expected, Switch1Devices.HasZeroBluetoothVendorId(path));

    [Theory]
    [InlineData("Lic Pro Controller", ControllerKind.Pro1)]
    [InlineData("lic pro controller", ControllerKind.Pro1)]
    [InlineData("Lic Pro Controller (Bluetooth)", ControllerKind.Pro1)]
    [InlineData("Pro Controller", ControllerKind.Unknown)]
    [InlineData("Wireless Controller", ControllerKind.Unknown)]
    [InlineData("", ControllerKind.Unknown)]
    [InlineData(null, ControllerKind.Unknown)]
    public void LizenzArt_AusProduktname(string? name, ControllerKind expected) =>
        Assert.Equal(expected, Switch1Devices.LicensedKind(name));
}
