using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Switch2Pro.Bridge;

/// <summary>Modellbilder und Offline-Varianten für Kodi aus derselben Bildquelle exportieren.</summary>
internal static class ControllerIconExporter
{
    public static void Run(string folder)
    {
        Directory.CreateDirectory(folder);
        var assembly = typeof(InputView).Assembly;
        foreach (string resource in assembly.GetManifestResourceNames().Where(n => n.StartsWith("ControllerArt.", StringComparison.Ordinal)))
        {
            using var stream = assembly.GetManifestResourceStream(resource)!;
            using var source = new Bitmap(stream);
            using var pixels = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(pixels)) g.DrawImageUnscaled(source, 0, 0);
            var data = pixels.LockBits(new Rectangle(0, 0, pixels.Width, pixels.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bytes = new byte[data.Stride * data.Height];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            pixels.UnlockBits(data);
            int left = pixels.Width, top = pixels.Height, right = 0, bottom = 0;
            for (int y = 0; y < pixels.Height; y++)
            for (int x = 0; x < pixels.Width; x++)
                if (bytes[y * data.Stride + x * 4 + 3] >= 16)
                {
                    left = Math.Min(left, x); right = Math.Max(right, x);
                    top = Math.Min(top, y); bottom = Math.Max(bottom, y);
                }
            var crop = Rectangle.FromLTRB(Math.Max(0, left - 4), Math.Max(0, top - 4),
                Math.Min(pixels.Width, right + 5), Math.Min(pixels.Height, bottom + 5));
            float scale = Math.Min(112f / crop.Width, 112f / crop.Height);
            var target = new RectangleF((128 - crop.Width * scale) / 2, (128 - crop.Height * scale) / 2,
                crop.Width * scale, crop.Height * scale);
            using var icon = new Bitmap(128, 128, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(icon))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(pixels, target, crop, GraphicsUnit.Pixel);
            }
            string name = resource["ControllerArt.".Length..];
            icon.Save(Path.Combine(folder, name), ImageFormat.Png);
            using var off = new Bitmap(128, 128, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(off))
            using (var attr = new ImageAttributes())
            {
                attr.SetColorMatrix(new ColorMatrix([
                    [0.30f, 0.30f, 0.30f, 0, 0], [0.59f, 0.59f, 0.59f, 0, 0], [0.11f, 0.11f, 0.11f, 0, 0],
                    [0, 0, 0, 0.40f, 0], [0, 0, 0, 0, 1],
                ]));
                g.DrawImage(icon, new Rectangle(0, 0, 128, 128), 0, 0, 128, 128, GraphicsUnit.Pixel, attr);
            }
            off.Save(Path.Combine(folder, name.Replace(".png", "_off.png", StringComparison.Ordinal)), ImageFormat.Png);
        }
        var aliases = new Dictionary<string, string> { ["pro"] = "pro2", ["gamepad"] = "pro1", ["playstation"] = "dualshock4",
            ["joycon_l"] = "jc2_links_hochkant", ["joycon_r"] = "jc2_rechts_hochkant", ["joycon_pair"] = "jc2_paar" };
        foreach (var (alias, source) in aliases)
        foreach (string suffix in new[] { ".png", "_off.png" })
            File.Copy(Path.Combine(folder, source + suffix), Path.Combine(folder, alias + suffix), overwrite: true);
    }
}
