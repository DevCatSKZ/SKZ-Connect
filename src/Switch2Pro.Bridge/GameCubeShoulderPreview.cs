using System.Drawing.Imaging;
using Switch2Pro.Protocol;

namespace Switch2Pro.Bridge;

internal static class GameCubeShoulderPreview
{
    public static void Run(string folder)
    {
        Directory.CreateDirectory(folder);
        Theme.Init("dark", transparency: false, "windows");
        foreach (int width in new[] { 580, 1160 })
        foreach (var (name, buttons, trigger) in new[]
        {
            ("idle", ProButtons.None, 0f), ("L", ProButtons.L, 0f), ("R", ProButtons.R, 0f),
            ("R-half", ProButtons.None, 0.5f), ("ZL", ProButtons.ZL, 0f), ("Z", ProButtons.ZR, 0f),
            ("all", ProButtons.L | ProButtons.R | ProButtons.ZL | ProButtons.ZR, 0f),
        })
        {
            using var view = new InputView { Size = new Size(width, width * 410 / 580) };
            var input = new PadInput { Kind = ControllerKind.GameCube2, Buttons = buttons, RightTrigger = trigger };
            view.Show(input, Mapping.ToGamepad(input, new Settings()));
            using var bitmap = new Bitmap(view.Width, view.Height);
            view.DrawToBitmap(bitmap, view.ClientRectangle);
            bitmap.Save(Path.Combine(folder, $"gamecube-{name}-{width}.png"), ImageFormat.Png);
        }
    }
}
