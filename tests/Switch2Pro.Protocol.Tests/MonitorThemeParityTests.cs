using System.Text.Json;
using Switch2Pro.Protocol;

namespace Switch2Pro.Protocol.Tests;

public class MonitorThemeParityTests
{
    [Fact]
    public void EveryPaletteMatchesTheMonitorReference()
    {
        using var stream = typeof(MonitorThemeParityTests).Assembly.GetManifestResourceStream("MonitorThemes.json")!;
        using var reference = JsonDocument.Parse(stream);
        Assert.Equal(ColorSchemes.All.Select(s => s.Id), reference.RootElement.EnumerateArray().Select(s => s.GetProperty("id").GetString()));
        foreach (var source in reference.RootElement.EnumerateArray())
        {
            var scheme = ColorSchemes.ById(source.GetProperty("id").GetString());
            Assert.Equal(source.GetProperty("name").GetString(), scheme.Name);
            foreach (var mode in new[] { "dark", "light" })
            {
                var p = mode == "dark" ? scheme.DarkPalette : scheme.LightPalette;
                var expected = source.GetProperty(mode);
                var slots = new Dictionary<string, int> { ["window"] = p.Window, ["surface"] = p.Surface,
                    ["control"] = p.SurfaceHover, ["border"] = p.Border, ["text"] = p.Text, ["muted"] = p.TextMuted,
                    ["canvas"] = p.Canvas, ["controlBorder"] = p.ControlBorder, ["accent"] = p.Accent,
                    ["accent2"] = p.Accent2, ["pressed"] = p.Pressed };
                foreach (var (key, value) in slots)
                    Assert.True(expected.GetProperty(key).GetInt32() == value, $"{scheme.Id}/{mode}/{key}: {value:X6} != {expected.GetProperty(key).GetInt32():X6}");
            }
            if (source.TryGetProperty("onAccent", out var onAccent))
                Assert.Equal(onAccent.GetInt32(), ColorSchemeDecoration.For(scheme.Id)!.OnAccent);
        }
        Assert.Equal(6, ColorSchemeDecoration.For("nova")!.Holographic!.Count);
    }
}
