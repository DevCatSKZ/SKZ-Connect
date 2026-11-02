using System.Drawing;
using System.Drawing.Drawing2D;
using Switch2Pro.Protocol;

namespace Switch2Pro.Bridge;

internal static partial class Theme
{
    private static readonly Dictionary<(string Scheme, bool Dark, int Player), PlayerBadgeColors.Badge> BadgeCache = [];
    internal static void PaintPlayerBadge(Graphics g, RectangleF bounds, int playerIndex, Font font)
    {
        var key = (ActiveScheme.Id, Dark, Math.Clamp(playerIndex, 0, 7));
        if (!BadgeCache.TryGetValue(key, out var colors))
            BadgeCache[key] = colors = PlayerBadgeColors.For(ColorSchemes.ById(ActiveScheme.Id), Dark, key.Item3);
        var color = C(colors.Fill);
        using (var fill = new SolidBrush(color)) g.FillEllipse(fill, bounds);
        using (var edge = new Pen(Blend(color, Current.Text, 0.24f), UiScale.Px(1f))) g.DrawEllipse(edge, bounds);
        TextRenderer.DrawText(g, (playerIndex + 1).ToString(), font, Rectangle.Round(bounds), C(colors.Text),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }
    internal static void MapGradient(LinearGradientBrush brush, Func<Color, Color> map)
    {
        var blend = brush.InterpolationColors;
        blend.Colors = blend.Colors.Select(map).ToArray();
        brush.InterpolationColors = blend;
    }
    internal static ColorSchemeDecoration? Decoration => ColorSchemeDecoration.For(ActiveScheme.Id);
    private static ControlAppearance Appearance(bool active = false, float hover = 0, bool pressed = false, bool enabled = true) =>
        ControlAppearance.For(ColorSchemes.ById(ActiveScheme.Id), Dark, active, hover, pressed, enabled);
    internal static Color CardBorder => C(Appearance().CardBorder);
    internal static Color GlassSurface => C(Appearance().Glass);

    internal static Color[] GradientColors => Dark && Decoration?.Holographic is { } holo
        ? holo.Select(C).ToArray()
        : Dark && Decoration?.Prism == true ? [Accent, Accent2, Current.Pressed] : [Accent, Accent2];

    private static Color[] ButtonColors(bool active, float hover, bool pressed, bool enabled)
    {
        var style = Appearance(active, hover, pressed, enabled);
        return [C(style.FillStart), C(style.FillEnd)];
    }

    internal static Color ButtonText(bool active, bool enabled = true) => C(Appearance(active, 1, false, enabled).Text);

    internal static float Contrast(Color a, Color b) =>
        (Math.Max(Luminance(a), Luminance(b)) + 0.05f) / (Math.Min(Luminance(a), Luminance(b)) + 0.05f);

    internal static void PaintButton(Graphics g, GraphicsPath path, RectangleF bounds, bool active = false,
        float hover = 0, bool pressed = false, bool enabled = true)
    {
        var colors = ButtonColors(active, hover, pressed, enabled);
        using (var fill = new LinearGradientBrush(bounds, colors[0], colors[1], 25f))
            g.FillPath(fill, path);
        if (active || hover > 0.01f)
            Glow(g, path, active ? 0.16f : 0.08f * hover, 3);
        using var border = LineBrush(bounds, 25f);
        var blend = border.InterpolationColors;
        float strength = !enabled ? 0.12f : active ? 0.95f : 0.38f + hover * 0.25f;
        blend.Colors = blend.Colors.Select(c => Blend(Current.ControlBorder, c, strength)).ToArray();
        border.InterpolationColors = blend;
        using var edge = new Pen(border, UiScale.Px(active ? 1.6f : 1.2f));
        g.DrawPath(edge, path);
    }
}
