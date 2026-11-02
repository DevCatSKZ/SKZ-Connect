using System.Drawing;
using System.Drawing.Drawing2D;
using Switch2Pro.Protocol;

namespace Switch2Pro.Bridge;

internal sealed partial class InputView
{
    private bool _artworkVisible;
    private static readonly Dictionary<string, Image> ArtworkCache = [];

    // Die Vorlagen wurden bei 580 × 410 gerendert. Die Zeichenfläche ist 580 × 430;
    // diese inverse Abbildung erhält die ursprünglichen Tastenkoordinaten exakt.
    private static readonly RectangleF ArtworkCanvas = new((W - W * H / 410f) / 2, 0, W * H / 410f, H);

    internal static string ArtworkName(ControllerKind kind, WiiExtension extension = WiiExtension.None) =>
        ControllerArtwork.Name(kind, extension);

    private string CurrentArtworkName()
    {
        if (_joyCons is not { Count: > 0 } parts)
            return ArtworkName(_input?.Kind ?? ControllerKind.Pro2, WiiExtension);
        var first = parts.FirstOrDefault(p => p.Kind.IsLeftJoyCon()) ?? parts[0];
        var tag = first.Kind.IsSwitch2() ? "jc2" : "jc1";
        if (parts.Count == 2)
            return tag + (parts.All(p => p.InGrip) ? "_grip" : "_paar");
        return tag + (first.Kind.IsLeftJoyCon() ? "_links" : "_rechts")
            + (first.Upright ? "_hochkant" : "_quer");
    }

    private ArtworkLayer BeginArtwork(Graphics g)
    {
        var name = CurrentArtworkName();
        if (!ArtworkCache.TryGetValue(name, out var image))
        {
            using var stream = typeof(InputView).Assembly.GetManifestResourceStream($"ControllerArt.{name}.png");
            if (stream is null)
                return default; // Vektor-Rückfall für unvollständige Entwicklungs-Builds.
            using var source = Image.FromStream(stream);
            ArtworkCache[name] = image = new Bitmap(source);
        }
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.DrawImage(image, ArtworkCanvas);
        _artworkVisible = true;
        var state = g.Save();
        g.SetClip(RectangleF.Empty);
        return new ArtworkLayer(this, g, state);
    }

    // Nur aktive Bedienelemente über das Bild zeichnen. Das gespeicherte Graphics-
    // Transform bleibt erhalten, auch bei gedrehten Joy-Con und Wii-Erweiterungen.
    private ControlLayer RevealControl(Graphics g, bool active)
    {
        if (!_artworkVisible || !active)
            return default;
        var state = g.Save();
        g.ResetClip();
        return new ControlLayer(g, state);
    }

    private static void HighlightArtwork(Graphics g, GraphicsPath path, float value = 1)
    {
        using var glow = new Pen(AccentGlow, 5f);
        g.DrawPath(glow, path);
        var state = g.Save();
        g.SetClip(path, CombineMode.Intersect);
        var bounds = path.GetBounds();
        using (var tint = new SolidBrush(Color.FromArgb(105, Accent)))
            g.FillRectangle(tint, bounds.X, bounds.Y, bounds.Width * Math.Clamp(value, 0, 1), bounds.Height);
        g.Restore(state);
        using var edge = new Pen(Accent, 1.8f);
        g.DrawPath(edge, path);
    }

    // Konturen direkt am neuen GameCube-Bild (1491 × 1055) abgenommen.
    // Die alten Foto-Keile passen nicht zu seinen abgerundeten Schultertasten.
    private static GraphicsPath GameCubeArtworkShoulder(string button)
    {
        var path = new GraphicsPath();
        bool mirror = button is "R" or "Z";
        PointF P(float x, float y) => new(
            ArtworkCanvas.X + (mirror ? 1491 - x : x) / 1491f * ArtworkCanvas.Width,
            ArtworkCanvas.Y + y / 1055f * ArtworkCanvas.Height);
        if (button is "L" or "R")
        {
            path.AddBezier(P(423, 104), P(464, 77), P(505, 42), P(547, 35));
            path.AddBezier(P(547, 35), P(578, 27), P(631, 35), P(652, 47));
            path.AddBezier(P(652, 47), P(664, 52), P(667, 60), P(665, 73));
            path.AddLine(P(665, 73), P(662, 111));
            path.AddBezier(P(662, 111), P(661, 124), P(658, 132), P(647, 137));
            path.AddBezier(P(647, 137), P(587, 165), P(500, 179), P(434, 166));
            path.AddBezier(P(434, 166), P(425, 165), P(423, 160), P(423, 150));
        }
        else
        {
            path.AddBezier(P(300, 173), P(310, 140), P(324, 111), P(341, 91));
            path.AddBezier(P(341, 91), P(352, 77), P(368, 77), P(385, 82));
            path.AddBezier(P(385, 82), P(403, 87), P(411, 100), P(410, 121));
            path.AddLine(P(410, 121), P(408, 157));
            path.AddBezier(P(408, 157), P(397, 190), P(365, 223), P(335, 229));
            path.AddBezier(P(335, 229), P(315, 231), P(300, 219), P(299, 201));
        }
        path.CloseFigure();
        return path;
    }

    private readonly struct ControlLayer(Graphics graphics, GraphicsState state) : IDisposable
    {
        public void Dispose()
        {
            if (graphics is not null) graphics.Restore(state);
        }
    }

    private readonly struct ArtworkLayer(InputView view, Graphics graphics, GraphicsState state) : IDisposable
    {
        public void Dispose()
        {
            if (view is null) return;
            graphics.Restore(state);
            view._artworkVisible = false;
        }
    }
}
