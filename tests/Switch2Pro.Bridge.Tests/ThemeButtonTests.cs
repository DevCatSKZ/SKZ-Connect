using System.Drawing;
using System.Drawing.Imaging;

namespace Switch2Pro.Bridge.Tests;

[Collection(LanguageCollection.Name)]
public class ThemeButtonTests
{
    [Fact]
    public void EnabledButtonTextIsReadableAcrossEveryThemeAndState()
    {
        string previous = Theme.ActiveScheme.Id;
        bool dark = Theme.Dark;
        try
        {
            foreach (var scheme in Theme.Schemes)
            foreach (string mode in new[] { "dark", "light" })
            foreach (bool active in new[] { false, true })
            foreach (bool down in new[] { false, true })
            {
                Theme.Init(mode, false, scheme.Id);
                using var bitmap = new Bitmap(200, 60, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Theme.GlassSurface);
                    var rect = new RectangleF(2, 2, 196, 56);
                    using var path = Theme.RoundedRect(rect, 8);
                    Theme.PaintButton(g, path, rect, active, 1, down);
                }
                var text = Theme.ButtonText(active);
                foreach (int x in new[] { 20, 100, 180 })
                    Assert.True(Theme.Contrast(text, bitmap.GetPixel(x, 30)) >= 4.5f,
                        $"{scheme.Id}/{mode}/active={active}/down={down}: text contrast at {x}");
                // Ein Glow darf außerhalb des Buttons keinen deckenden Neonrahmen zeichnen.
                using var glowImage = new Bitmap(200, 60);
                using (var g = Graphics.FromImage(glowImage))
                {
                    g.Clear(Theme.GlassSurface);
                    var rect = new RectangleF(12, 12, 176, 36);
                    using var path = Theme.RoundedRect(rect, 6);
                    Theme.PaintButton(g, path, rect, active: true);
                }
                var outside = glowImage.GetPixel(10, 30);
                Assert.InRange(Math.Abs(outside.R - Theme.GlassSurface.R), 0, 40);
                Assert.InRange(Math.Abs(outside.G - Theme.GlassSurface.G), 0, 40);
                Assert.InRange(Math.Abs(outside.B - Theme.GlassSurface.B), 0, 40);
            }
        }
        finally { Theme.Init(dark ? "dark" : "light", false, previous); }
    }
}
