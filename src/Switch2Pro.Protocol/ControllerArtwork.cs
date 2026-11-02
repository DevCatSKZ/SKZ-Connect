namespace Switch2Pro.Protocol;

/// <summary>Gemeinsame Namen der Controller-Bilddateien für alle Oberflächen.</summary>
public static class ControllerArtwork
{
    public static string Name(ControllerKind kind, WiiExtension extension = WiiExtension.None) => kind switch
    {
        ControllerKind.Pro2 => "pro2", ControllerKind.Pro1 or ControllerKind.Unknown => "pro1",
        ControllerKind.GameCube2 => "gamecube", ControllerKind.SnesController => "snes",
        ControllerKind.NesController => "nes", ControllerKind.N64Controller => "n64", ControllerKind.MegaDrive => "megadrive",
        ControllerKind.WiiUPro => "wiiupro", ControllerKind.DualShock4 => "dualshock4", ControllerKind.DualSense => "dualsense",
        ControllerKind.XboxController => "xbox",
        ControllerKind.WiiRemote when extension.HasClassic() => "wii_classic",
        ControllerKind.WiiRemote when extension.HasNunchuk() && extension.HasMotionPlus() => "wii_motionplus_nunchuk",
        ControllerKind.WiiRemote when extension.HasNunchuk() => "wii_nunchuk",
        ControllerKind.WiiRemote when extension.HasMotionPlus() => "wii_motionplus", ControllerKind.WiiRemote => "wii",
        ControllerKind.JoyCon1Left => "jc1_links_hochkant", ControllerKind.JoyCon1Right => "jc1_rechts_hochkant",
        ControllerKind.JoyCon2Left => "jc2_links_hochkant", ControllerKind.JoyCon2Right => "jc2_rechts_hochkant",
        ControllerKind.JoyConPair => "jc2_paar", _ => "pro1",
    };

    public static string JoyCon(ControllerKind kind, bool upright = true, bool pair = false, bool grip = false) =>
        (kind.IsSwitch2() ? "jc2" : "jc1") + (pair ? grip ? "_grip" : "_paar"
            : (kind.IsLeftJoyCon() ? "_links" : "_rechts") + (upright ? "_hochkant" : "_quer"));
}
