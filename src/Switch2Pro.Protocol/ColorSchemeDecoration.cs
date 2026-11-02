namespace Switch2Pro.Protocol;

/// <summary>Glas- und Prisma-Eigenschaften aus SKZ Monitor/wwwroot/themes.js.
/// Die RGB-Paletten bleiben die gemeinsame Quelle für alle Plattformen.</summary>
public sealed record ColorSchemeDecoration(uint Glass, uint Ring, int OnAccent, bool Prism = false,
    IReadOnlyList<int>? Holographic = null)
{
    public static ColorSchemeDecoration? For(string id) => id switch
    {
        "hologlas" => new(0x123EE6D0, 0x3830E6C8, 0x04211C, true),
        "mitternacht" => new(0x14FFFFFF, 0x26FFFFFF, 0xFFFFFF),
        "tageslicht" => new(0xB8FFFFFF, 0x22132033, 0xFFFFFF),
        "nova" => new(0x0FFFFFFF, 0x1FFFFFFF, 0x020407, true,
            [0x2DE1C2, 0x3CC4FF, 0x7C5CFF, 0xE86CC8, 0xFFC24B, 0xFFE296]),
        "abendrot" => new(0x14FFB37A, 0x38FFB37A, 0x2A1206, true),
        "tiefsee" => new(0x1222E3F0, 0x3822E3F0, 0x02161C, true),
        "sakura" => new(0x12FF8FB8, 0x38FF8FB8, 0x2A0716, true),
        "graphit" => new(0x0DFFFFFF, 0x22FFFFFF, 0x050506),
        "samtpfoten" => new(0x14FFC857, 0x38FFC857, 0x241A02, true),
        _ => null,
    };
}
