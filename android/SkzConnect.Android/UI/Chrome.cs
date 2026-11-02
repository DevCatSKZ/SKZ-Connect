using Android.Views;
using AndroidX.Core.View;
using Google.Android.Material.AppBar;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.ProgressIndicator;
using Switch2Pro.Protocol;
using View = Android.Views.View;

namespace SkzConnect.Android.UI;

/// <summary>Fensterrahmen im Farbschema: Hintergrund, Titelleiste, untere Navigation, Statusleisten-Symbole.</summary>
public static class Chrome
{
    public static void Apply(global::Android.App.Activity activity, View root, MaterialToolbar toolbar,
        BottomNavigationView? nav = null, LinearProgressIndicator? progress = null)
    {
        var p = Look.P(activity);
        root.SetBackgroundColor(Look.C(p.Window));
        toolbar.SetBackgroundColor(Look.C(p.Window));
        toolbar.SetTitleTextColor(Look.C(p.Text));
        if (nav is not null)
        {
            nav.SetBackgroundColor(Look.C(p.Dark ? p.Window : p.Surface));
            var tint = Look.States(Look.Style(activity, true).Text, p.TextMuted);
            nav.ItemIconTintList = tint;
            nav.ItemTextColor = tint;
            nav.ItemActiveIndicatorColor = Look.Solid(Look.Style(activity, true).FillStart);
        }
        if (progress is not null)
        {
            progress.SetIndicatorColor(Look.C(Look.AccentLine(activity)).ToArgb());
            progress.TrackColor = Look.C(p.SurfaceHover).ToArgb();
        }
        // Symbole in Status- und Navigationsleiste: dunkel auf hellem Grund, hell auf dunklem.
        var controller = WindowCompat.GetInsetsController(activity.Window!, activity.Window!.DecorView)!;
        controller.AppearanceLightStatusBars = !p.Dark;
        controller.AppearanceLightNavigationBars = !p.Dark;
    }
}
