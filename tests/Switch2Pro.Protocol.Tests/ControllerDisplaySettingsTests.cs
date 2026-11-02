using System.Text.Json;
using Switch2Pro.Protocol;
using Xunit;

namespace Switch2Pro.Protocol.Tests;

public sealed class ControllerDisplaySettingsTests
{
    [Fact]
    public void NewAndUnspecifiedSettingsUseWindowsWhileSavedSelectionsRemain()
    {
        Assert.Equal("windows", new Settings().ColorScheme);
        Assert.Equal("windows", JsonSerializer.Deserialize<Settings>("{}")!.ColorScheme);
        Assert.Equal("windows", ColorSchemes.ById(JsonSerializer.Deserialize<Settings>("{\"ColorScheme\":null}")!.ColorScheme).Id);
        var selected = new Settings { ColorScheme = "joycon" };
        var loaded = JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(selected))!;
        Assert.Equal("joycon", loaded.ColorScheme);
        Assert.Equal("JoCo", ColorSchemes.ById(loaded.ColorScheme).Name);
    }

    [Fact]
    public void OldSettingsKeepImagesAndButtonOnlyPreferenceSurvivesReloadAndCopy()
    {
        Assert.True(JsonSerializer.Deserialize<Settings>("{}")!.ShowControllerImages);
        var selected = new Settings { ShowControllerImages = false };
        var loaded = JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(selected))!;
        Assert.False(loaded.ShowControllerImages);
        var running = new Settings();
        running.CopyFrom(loaded);
        Assert.False(running.ShowControllerImages);
        running.CopyFrom(new Settings());
        Assert.True(running.ShowControllerImages);
    }
}
