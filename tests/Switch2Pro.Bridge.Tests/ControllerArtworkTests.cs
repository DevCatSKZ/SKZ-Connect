using System.Drawing;
using System.Drawing.Imaging;
using Switch2Pro.Bridge;
using Switch2Pro.Protocol;

namespace Switch2Pro.Bridge.Tests;

public sealed class ControllerArtworkTests
{
    [Fact]
    public void EveryControllerAndWiiExtensionHasEmbeddedArtwork()
    {
        var names = Enum.GetValues<ControllerKind>().Select(k => InputView.ArtworkName(k))
            .Concat(Enum.GetValues<WiiExtension>().Select(e => InputView.ArtworkName(ControllerKind.WiiRemote, e)))
            .Concat(new[] { "jc1", "jc2" }.SelectMany(tag =>
                new[] { "links_hochkant", "rechts_hochkant", "links_quer", "rechts_quer", "paar", "grip" }
                    .Select(mode => $"{tag}_{mode}"))).Distinct();
        foreach (var name in names)
        {
            using var stream = typeof(InputView).Assembly.GetManifestResourceStream($"ControllerArt.{name}.png");
            Assert.NotNull(stream);
            using var image = new Bitmap(stream);
            Assert.True(image.Width >= 1000 && image.Height >= 700, name);
            Assert.InRange((double)image.Width / image.Height, 580d / 410 - 0.02, 580d / 410 + 0.02);
            Assert.True(Image.IsAlphaPixelFormat(image.PixelFormat), name);
            Assert.InRange(image.GetPixel(0, 0).A, 0, 2);
        }
    }

    [Fact]
    public void PressedButtonsChangeArtworkAndReleaseRestoresIt()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                foreach (var kind in new[] { ControllerKind.Pro2, ControllerKind.DualSense, ControllerKind.GameCube2,
                             ControllerKind.SnesController, ControllerKind.WiiRemote })
                {
                    using var view = new InputView { Size = new Size(580, 410) };
                    byte[] Render(ProButtons buttons)
                    {
                        var input = new PadInput { Kind = kind, Buttons = buttons };
                        view.Show(input, Mapping.ToGamepad(input, new Settings()));
                        using var bitmap = new Bitmap(view.Width, view.Height);
                        view.DrawToBitmap(bitmap, view.ClientRectangle);
                        using var output = new MemoryStream();
                        bitmap.Save(output, ImageFormat.Png);
                        return output.ToArray();
                    }
                    var idle = Render(ProButtons.None);
                    var pressed = Render(ProButtons.A | ProButtons.Plus);
                    var released = Render(ProButtons.None);
                    Assert.False(idle.SequenceEqual(pressed), kind.ToString());
                    Assert.True(idle.SequenceEqual(released), kind.ToString());
                }
                foreach (var kind in new[] { ControllerKind.JoyCon1Left, ControllerKind.JoyCon1Right,
                             ControllerKind.JoyCon2Left, ControllerKind.JoyCon2Right })
                foreach (bool upright in new[] { false, true })
                {
                    using var view = new InputView { Size = new Size(580, 410) };
                    byte[] Render(ProButtons buttons)
                    {
                        var part = new InputView.JoyConPart(kind, new ControllerState { Buttons = buttons },
                            DeviceCalibration.Default, new Switch2Pro.Bridge.Links.ControllerInfo(), false, Upright: upright);
                        view.ShowJoyCons([part], new PadInput { Kind = kind, Buttons = buttons });
                        using var bitmap = new Bitmap(view.Width, view.Height);
                        view.DrawToBitmap(bitmap, view.ClientRectangle);
                        using var output = new MemoryStream();
                        bitmap.Save(output, ImageFormat.Png);
                        return output.ToArray();
                    }
                    var idle = Render(ProButtons.None);
                    Assert.False(idle.SequenceEqual(Render(ProButtons.A | ProButtons.Up | ProButtons.SLLeft | ProButtons.SLRight)));
                    Assert.True(idle.SequenceEqual(Render(ProButtons.None)));
                }
                byte[] MovingXbox(int reportedBody)
                {
                    using var view = new InputView { Size = new Size(580, 410) };
                    view.SetColors(reportedBody, 0xCCCCCC, null);
                    var input = new PadInput { Kind = ControllerKind.XboxController, LeftX = 0.7f, RightY = -0.6f };
                    view.Show(input, Mapping.ToGamepad(input, new Settings()));
                    using var bitmap = new Bitmap(view.Width, view.Height);
                    view.DrawToBitmap(bitmap, view.ClientRectangle);
                    using var output = new MemoryStream();
                    bitmap.Save(output, ImageFormat.Png);
                    return output.ToArray();
                }
                // Das feste Produktbild und bewegte Stickkappen müssen dieselbe
                // Materialfarbe behalten, auch wenn ein Gerät eine andere meldet.
                Assert.True(MovingXbox(0xFF2020).SequenceEqual(MovingXbox(0x303030)));
                using (var view = new InputView { Size = new Size(580, 410), ButtonsOnly = true })
                {
                    byte[] Grid(ProButtons buttons)
                    {
                        var input = new PadInput { Kind = ControllerKind.GameCube2, Buttons = buttons };
                        view.Show(input, Mapping.ToGamepad(input, new Settings()));
                        using var bitmap = new Bitmap(view.Width, view.Height);
                        view.DrawToBitmap(bitmap, view.ClientRectangle);
                        // Das bisherige violette Gehäuse darf in der Tastenansicht nicht auftauchen.
                        Assert.Equal(Theme.GlassSurface.ToArgb(), bitmap.GetPixel(0, view.Height - 1).ToArgb());
                        using var output = new MemoryStream();
                        bitmap.Save(output, ImageFormat.Png);
                        return output.ToArray();
                    }
                    var idle = Grid(ProButtons.None);
                    var active = Grid(ProButtons.B);
                    Assert.True(view.ButtonIndicators().Single(b => b.Label == "A").Active);
                    Assert.False(view.ButtonIndicators().Single(b => b.Label == "B").Active);
                    Assert.False(idle.SequenceEqual(active));
                    Assert.True(idle.SequenceEqual(Grid(ProButtons.None)));
                    view.ButtonsOnly = false;
                    Assert.False(idle.SequenceEqual(Grid(ProButtons.None)));
                    view.ButtonsOnly = true;
                    Assert.True(idle.SequenceEqual(Grid(ProButtons.None)));
                    var analog = new PadInput { Kind = ControllerKind.XboxController, LeftTrigger = 0.65f };
                    view.Show(analog, Mapping.ToGamepad(analog, new Settings()));
                    Assert.True(view.ButtonIndicators().Single(b => b.Label == "LT").Active);
                    Assert.Equal(0.65f, view.ButtonIndicators().Single(b => b.Label == "LT").Level);
                    var n64 = new PadInput { Kind = ControllerKind.N64Controller, RightY = 1 };
                    view.Show(n64, Mapping.ToGamepad(n64, new Settings()));
                    Assert.True(view.ButtonIndicators().Single(b => b.Label == "C ↑").Active);
                    var left = new InputView.JoyConPart(ControllerKind.JoyCon2Left,
                        new ControllerState { Buttons = ProButtons.Up }, DeviceCalibration.Default,
                        new Switch2Pro.Bridge.Links.ControllerInfo(), false, Upright: true);
                    view.ShowJoyCons([left], new PadInput { Kind = left.Kind, Buttons = ProButtons.Up });
                    Assert.True(view.ButtonIndicators().Single(b => b.Label == "↑").Active);
                    Assert.Contains(view.ButtonIndicators(), b => b.Label == "SL");
                    Assert.DoesNotContain(view.ButtonIndicators(), b => b.Label == "A");
                    view.WiiExtension = WiiExtension.None;
                    view.Show(new PadInput { Kind = ControllerKind.WiiRemote }, default);
                    Assert.DoesNotContain(view.ButtonIndicators(), b => b.Label.StartsWith("Classic", StringComparison.Ordinal));
                    Assert.DoesNotContain(view.ButtonIndicators(), b => b.Label == "C");
                    view.WiiExtension = WiiExtension.Nunchuk;
                    Assert.Contains(view.ButtonIndicators(), b => b.Label == "C");
                    Assert.Contains(view.ButtonIndicators(), b => b.Label == "Z");
                }
            }
            catch (Exception ex) { failure = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(30)));
        if (failure is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
    }
}
