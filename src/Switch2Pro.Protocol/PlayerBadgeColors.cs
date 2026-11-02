namespace Switch2Pro.Protocol;

/// <summary>Unterscheidbare Spielerfarben aus dem gewählten Theme, mit lesbarer Ziffer.</summary>
public static class PlayerBadgeColors
{
    public readonly record struct Badge(int Fill, int Text);

    public static Badge For(ColorScheme scheme, bool dark, int playerIndex)
    {
        var palette = dark ? scheme.DarkPalette : scheme.LightPalette;
        int[] primary = [palette.Accent, palette.Accent2, palette.Pressed];
        var chosen = new List<int> { primary[0] };
        for (int i = 1; i <= Math.Clamp(playerIndex, 0, 7); i++)
        {
            int candidate = i < primary.Length ? primary[i] : palette.Accent;
            if (chosen.Min(c => Distance(c, candidate)) < 72 * 72)
                candidate = Enumerable.Range(0, 24).Select(n => Rotate(palette.Accent, n * 15f))
                    .OrderByDescending(c => chosen.Min(previous => Distance(previous, c))).First();
            chosen.Add(candidate);
        }
        int fill = chosen[^1];
        float luminance = ColorSchemes.Luminance(fill);
        int text = (luminance + 0.05f) / 0.05f >= 1.05f / (luminance + 0.05f) ? 0x000000 : 0xFFFFFF;
        return new(fill, text);
    }

    private static int Distance(int a, int b)
    {
        int r = ((a >> 16) & 255) - ((b >> 16) & 255), g = ((a >> 8) & 255) - ((b >> 8) & 255), blue = (a & 255) - (b & 255);
        return r * r + g * g + blue * blue;
    }

    private static int Rotate(int rgb, float degrees)
    {
        float r = ((rgb >> 16) & 255) / 255f, g = ((rgb >> 8) & 255) / 255f, b = (rgb & 255) / 255f;
        float max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b)), delta = max - min;
        float hue = delta < 0.001f ? 0 : max == r ? 60 * ((g - b) / delta % 6) : max == g ? 60 * ((b - r) / delta + 2) : 60 * ((r - g) / delta + 4);
        hue = (hue + degrees + 360) % 360;
        float saturation = Math.Clamp(max > 0 ? delta / max : 0, 0.48f, 0.78f), value = Math.Clamp(max, 0.62f, 0.9f);
        float c = value * saturation, x = c * (1 - Math.Abs(hue / 60 % 2 - 1)), m = value - c;
        var (red, green, blue) = hue switch
        {
            < 60 => (c, x, 0f), < 120 => (x, c, 0f), < 180 => (0f, c, x),
            < 240 => (0f, x, c), < 300 => (x, 0f, c), _ => (c, 0f, x),
        };
        return ((int)MathF.Round((red + m) * 255) << 16) | ((int)MathF.Round((green + m) * 255) << 8) | (int)MathF.Round((blue + m) * 255);
    }
}
