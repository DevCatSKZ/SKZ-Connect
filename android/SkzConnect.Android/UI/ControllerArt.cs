using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Switch2Pro.Protocol;
using View = Android.Views.View;

namespace SkzConnect.Android.UI;

/// <summary>Dieselben transparenten Modellbilder wie unter Windows, ohne zweite Bildquelle.</summary>
public static class ControllerArt
{
    private static readonly Dictionary<string, Bitmap> Cache = [];

    public static ImageView Image(Context context, string name, bool connected = true)
    {
        if (!Cache.TryGetValue(name, out var bitmap))
        {
            using var stream = context.Assets!.Open($"Controllers/{name}.png");
            bitmap = BitmapFactory.DecodeStream(stream, null, new BitmapFactory.Options { InSampleSize = 2 });
            if (bitmap is not null) Cache[name] = bitmap;
        }
        var image = new ImageView(context) { ContentDescription = L.T("Controller"),
            LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, Ui.Dp(context, 168)) };
        image.SetScaleType(ImageView.ScaleType.FitCenter);
        image.SetImageBitmap(bitmap);
        if (!connected)
        {
            var matrix = new ColorMatrix();
            matrix.SetSaturation(0);
            image.SetColorFilter(new ColorMatrixColorFilter(matrix));
            image.ImageAlpha = 102;
        }
        return image;
    }

    public static View Header(Context context, int playerIndex, string name)
    {
        var row = Ui.Column(context);
        row.Orientation = Orientation.Horizontal;
        row.SetGravity(global::Android.Views.GravityFlags.CenterVertical);
        var colors = PlayerBadgeColors.For(Look.Scheme, Look.IsDark(context), playerIndex);
        var badge = new TextView(context) { Text = (playerIndex + 1).ToString(), Gravity = global::Android.Views.GravityFlags.Center,
            LayoutParameters = new LinearLayout.LayoutParams(Ui.Dp(context, 32), Ui.Dp(context, 32)) { RightMargin = Ui.Dp(context, 10) } };
        badge.SetTextColor(Look.C(colors.Text));
        var background = new global::Android.Graphics.Drawables.GradientDrawable();
        background.SetShape(global::Android.Graphics.Drawables.ShapeType.Oval);
        background.SetColor(Look.C(colors.Fill));
        badge.Background = background;
        row.AddView(badge);
        row.AddView(Ui.Title(context, name));
        return row;
    }
}
