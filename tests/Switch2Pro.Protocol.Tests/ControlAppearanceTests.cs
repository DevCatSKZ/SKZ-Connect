using Switch2Pro.Protocol;

namespace Switch2Pro.Protocol.Tests;

public class ControlAppearanceTests
{
    [Fact]
    public void SharedWindowsAndAndroidButtonColorsHaveReadableText()
    {
        foreach (var scheme in ColorSchemes.All)
        foreach (bool dark in new[] { true, false })
        foreach (bool active in new[] { true, false })
        foreach (bool down in new[] { true, false })
        {
            var appearance = ControlAppearance.For(scheme, dark, active, 1, down);
            Assert.True(ControlAppearance.Contrast(appearance.Text, appearance.FillStart) >= 4.5f, scheme.Id);
            Assert.True(ControlAppearance.Contrast(appearance.Text, appearance.FillEnd) >= 4.5f, scheme.Id);
        }
        foreach (var kind in Enum.GetValues<ControllerKind>()) Assert.NotEmpty(ControllerArtwork.Name(kind));
        Assert.NotEqual(ControllerArtwork.Name(ControllerKind.Pro1), ControllerArtwork.Name(ControllerKind.Pro2));
        Assert.NotEqual(ControllerArtwork.JoyCon(ControllerKind.JoyCon1Left), ControllerArtwork.JoyCon(ControllerKind.JoyCon2Left));
    }
}
