using Switch2Pro.Protocol;

namespace Switch2Pro.Protocol.Tests;

public class PlayerBadgeColorTests
{
    [Fact]
    public void EveryThemeHasEightDistinctStablePlayerColorsWithReadableNumbers()
    {
        foreach (var scheme in ColorSchemes.All)
        foreach (bool dark in new[] { false, true })
        {
            var badges = Enumerable.Range(0, 8).Select(i => PlayerBadgeColors.For(scheme, dark, i)).ToArray();
            Assert.Equal(8, badges.Select(b => b.Fill).Distinct().Count());
            foreach (var badge in badges)
            {
                float l1 = ColorSchemes.Luminance(badge.Fill), l2 = ColorSchemes.Luminance(badge.Text);
                Assert.True((Math.Max(l1, l2) + 0.05f) / (Math.Min(l1, l2) + 0.05f) >= 4.5f, scheme.Id);
            }
            Assert.Equal(badges[0], PlayerBadgeColors.For(scheme, dark, 0));
            Assert.Equal(badges[7], PlayerBadgeColors.For(scheme, dark, 7));
        }
    }
}
