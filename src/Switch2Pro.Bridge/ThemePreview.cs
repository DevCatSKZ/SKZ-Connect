using System.Drawing;
using System.Drawing.Imaging;
using Switch2Pro.Protocol;

namespace Switch2Pro.Bridge;

/// <summary>Fensterfreie Vorschau aller Theme-Zustände für die Darstellungsprüfung.</summary>
internal static class ThemePreview
{
    public static void Run(string folder)
    {
        Directory.CreateDirectory(folder);
        foreach (var scheme in Theme.Schemes)
        foreach (var mode in new[] { "dark", "light" })
        {
            Theme.Init(mode, false, scheme.Id);
            using var bitmap = new Bitmap(1200, 800);
            using var g = Graphics.FromImage(bitmap);
            g.Clear(Theme.Backdrop);
            using (var ink = new SolidBrush(Theme.Current.Text))
                g.DrawString($"{scheme.Name} · {(Theme.Dark ? "Dunkel" : "Hell")}", UiFonts.Subtitle, ink, 20, 16);
            void Draw(Control control, Rectangle bounds, bool backdrop = false)
            {
                using (control)
                {
                    control.Size = bounds.Size;
                    control.BackColor = backdrop ? Theme.Backdrop : Theme.GlassSurface;
                    using var image = new Bitmap(bounds.Width, bounds.Height);
                    control.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
                    g.DrawImageUnscaled(image, bounds.Location);
                }
            }
            Draw(new GlyphButton("Einstellungen", Glyph.Settings), new Rectangle(20, 62, 170, 36));
            Draw(new GlyphButton("Einstellungen", Glyph.Settings) { Accent = true }, new Rectangle(205, 62, 170, 36));
            Draw(new GlyphButton("Einstellungen", Glyph.Settings) { Enabled = false }, new Rectangle(390, 62, 170, 36));
            Draw(new Segmented("Controllerbilder", "Nur Tasten"), new Rectangle(610, 62, 330, 36));
            var input = new PadInput { Kind = ControllerKind.Pro2, Buttons = ProButtons.A | ProButtons.Up | ProButtons.L | ProButtons.C,
                LeftX = 0.6f, RightY = -0.5f };
            var imageView = new InputView();
            imageView.Show(input, Mapping.ToGamepad(input, new Settings()));
            Draw(imageView, new Rectangle(20, 120, 570, 425));
            var buttons = new InputView { ButtonsOnly = true };
            buttons.Show(input, Mapping.ToGamepad(input, new Settings()));
            Draw(buttons, new Rectangle(610, 120, 570, 275));
            for (int player = 0; player < 8; player++)
                Theme.PaintPlayerBadge(g, new RectangleF(625 + player * 65, 425, 44, 44), player, UiFonts.Strong);
            Draw(new SchemePicker { SelectedIndex = Array.IndexOf(Theme.Schemes, scheme) }, new Rectangle(20, 585, 780, 180), true);
            bitmap.Save(Path.Combine(folder, $"{scheme.Id}-{mode}.png"), ImageFormat.Png);
        }
    }
}
