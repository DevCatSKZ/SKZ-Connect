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
        bool mirror = button == "Z";
        PointF P(float x, float y) => new(
            ArtworkCanvas.X + (mirror ? 1491 - x : x) / 1491f * ArtworkCanvas.Width,
            ArtworkCanvas.Y + y / 1055f * ArtworkCanvas.Height);
        if (button is "L" or "R")
        {
            // Die Kappen sind im Rasterbild nicht spiegelgleich. Beide Konturen
            // folgen separat der hellen Tastenfläche, knapp innerhalb ihrer Kante.
            PointF[] points = button == "L"
                ? [
                    P(570.5f, 32f), P(523f, 39f), P(504f, 46f), P(489f, 53f), P(476f, 60f),
                    P(463f, 67f), P(451f, 74f), P(440f, 81f), P(428f, 88f), P(423f, 95f),
                    P(422f, 102f), P(422f, 109f), P(422f, 116f), P(422f, 123f), P(422f, 130f),
                    P(422f, 137f), P(422f, 144f), P(422f, 151f), P(422f, 158f), P(426f, 165f),
                    P(443f, 168f), P(461f, 170f), P(483f, 171f), P(495f, 171f), P(512f, 170f),
                    P(525f, 168f), P(540f, 165f), P(564f, 158f), P(582f, 151f), P(598f, 144f),
                    P(613f, 137f), P(626f, 130f), P(638f, 123f), P(650f, 116f), P(658f, 109f),
                    P(660f, 102f), P(660f, 95f), P(661f, 88f), P(660f, 81f), P(661f, 74f),
                    P(667f, 67f), P(667f, 60f), P(664f, 53f), P(653f, 46f), P(626f, 39f),
                    P(570.5f, 32f)]
                : [
                    P(914f, 32f), P(861f, 39f), P(834f, 46f), P(825f, 53f), P(822f, 60f),
                    P(821f, 67f), P(821f, 74f), P(821f, 81f), P(828f, 88f), P(828f, 95f),
                    P(828f, 102f), P(830f, 109f), P(838f, 116f), P(850f, 123f), P(863f, 130f),
                    P(876f, 137f), P(891f, 144f), P(907f, 151f), P(924f, 158f), P(949f, 165f),
                    P(967f, 168f), P(984f, 170f), P(1004.5f, 171f), P(1004.5f, 171f), P(1024f, 170f),
                    P(1042f, 168f), P(1058f, 165f), P(1067f, 158f), P(1067f, 151f), P(1068f, 144f),
                    P(1067f, 137f), P(1068f, 130f), P(1068f, 123f), P(1068f, 116f), P(1067f, 109f),
                    P(1067f, 102f), P(1067f, 95f), P(1062f, 88f), P(1050f, 81f), P(1037f, 74f),
                    P(1025f, 67f), P(1011f, 60f), P(999f, 53f), P(983f, 46f), P(966f, 39f),
                    P(919f, 32f)];
            path.AddClosedCurve(points, 0.12f);
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
