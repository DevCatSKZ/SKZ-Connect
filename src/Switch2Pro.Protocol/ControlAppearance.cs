namespace Switch2Pro.Protocol;

/// <summary>Gemeinsame Glasflächen und lesbare Buttontexte für Windows und Android.</summary>
public readonly record struct ControlAppearance(int Glass, int CardBorder, int FillStart, int FillEnd,
    int BorderStart, int BorderEnd, int Text)
{
    public static ControlAppearance For(ColorScheme scheme, bool dark, bool active = false, float hover = 0,
        bool pressed = false, bool enabled = true)
    {
        var p = dark ? scheme.DarkPalette : scheme.LightPalette;
        var decoration = ColorSchemeDecoration.For(scheme.Id);
        int glass = p.Dark && decoration is { } d
            ? ColorSchemes.Blend(p.Surface, (int)(d.Glass & 0xFFFFFF), (d.Glass >> 24) / 255f)
            : ColorSchemes.Blend(p.Surface, p.Text, p.Dark ? 0.045f : 0.02f);
        int cardBorder = p.Dark && decoration is { } ring
            ? ColorSchemes.Blend(p.Border, (int)(ring.Ring & 0xFFFFFF), (ring.Ring >> 24) / 255f) : p.Border;
        int ground = ColorSchemes.Blend(glass, p.Window, 0.55f);
        float tint = !enabled ? 0.06f : pressed ? 0.14f : active ? 0.30f : 0.12f + 0.08f * hover;
        int start = ColorSchemes.Blend(ground, p.Accent, tint), end = ColorSchemes.Blend(ground, p.Accent2, tint);
        int accentLine = p.Dark && scheme.Neon ? ColorSchemes.Blend(p.Accent, 0xFFFFFF, 0.28f) : p.Accent;
        int accent2Line = p.Dark && scheme.Neon ? ColorSchemes.Blend(p.Accent2, 0xFFFFFF, 0.28f) : p.Accent2;
        int text = enabled ? p.Text : p.TextMuted;
        if (enabled && active)
            for (int step = 0; step <= 10; step++)
            {
                int candidate = ColorSchemes.Blend(accentLine, p.Text, step / 10f);
                if (Contrast(candidate, start) >= 4.5f && Contrast(candidate, end) >= 4.5f) { text = candidate; break; }
            }
        float strength = !enabled ? 0.12f : active ? 0.95f : 0.38f + hover * 0.25f;
        return new(glass, cardBorder, start, end, ColorSchemes.Blend(p.ControlBorder, accentLine, strength),
            ColorSchemes.Blend(p.ControlBorder, accent2Line, strength), text);
    }

    public static float Contrast(int a, int b) =>
        (Math.Max(ColorSchemes.Luminance(a), ColorSchemes.Luminance(b)) + 0.05f)
        / (Math.Min(ColorSchemes.Luminance(a), ColorSchemes.Luminance(b)) + 0.05f);
}
