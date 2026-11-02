using System.ComponentModel;
using System.Drawing;
using Switch2Pro.Protocol;

namespace Switch2Pro.Bridge;

internal sealed partial class InputView
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ButtonsOnly { get; set; }

    internal readonly record struct ButtonIndicator(string Label, bool Active, float? Level = null);

    internal IReadOnlyList<ButtonIndicator> ButtonIndicators()
    {
        var kind = _joyCons is { Count: 2 } ? ControllerKind.JoyConPair : _input?.Kind ?? ControllerKind.Pro2;
        bool upright = _joyCons is [{ Upright: true }];
        var keys = ControllerButtons.For(kind).ToList();
        var labelKind = kind;
        if (kind == ControllerKind.WiiRemote)
        {
            if (WiiExtension.HasClassic())
            {
                keys = ControllerButtons.For(ControllerKind.WiiUPro)
                    .Where(b => b is not ProButtons.LeftStick and not ProButtons.RightStick).ToList();
                labelKind = ControllerKind.Pro1;
            }
            else
            {
                keys = [ProButtons.A, ProButtons.Y, ProButtons.B, ProButtons.ZR, ProButtons.Minus,
                    ProButtons.Plus, ProButtons.Home, ProButtons.Up, ProButtons.Down, ProButtons.Left, ProButtons.Right];
                if (WiiExtension.HasNunchuk()) keys.AddRange([ProButtons.L, ProButtons.ZL]);
            }
        }
        if (kind is ControllerKind.GameCube2 or ControllerKind.XboxController)
        {
            keys.RemoveAll(b => b is ProButtons.A or ProButtons.B or ProButtons.X or ProButtons.Y);
            keys.InsertRange(0, kind == ControllerKind.GameCube2
                ? [ProButtons.B, ProButtons.Y, ProButtons.A, ProButtons.X]
                : [ProButtons.B, ProButtons.A, ProButtons.Y, ProButtons.X]);
        }
        if (upright)
        {
            bool left = _joyCons![0].Kind.IsLeftJoyCon();
            keys = left
                ? [ProButtons.Up, ProButtons.Down, ProButtons.Left, ProButtons.Right, ProButtons.L, ProButtons.ZL,
                    ProButtons.Minus, ProButtons.Capture, ProButtons.LeftStick, ProButtons.SLLeft, ProButtons.SRLeft]
                : [ProButtons.A, ProButtons.B, ProButtons.X, ProButtons.Y, ProButtons.R, ProButtons.ZR,
                    ProButtons.Plus, ProButtons.Home, ProButtons.RightStick, ProButtons.SLRight, ProButtons.SRRight];
            if (!left && _joyCons[0].Kind.IsSwitch2()) keys.Add(ProButtons.C);
        }
        if (kind == ControllerKind.XboxController)
            keys.AddRange([ProButtons.ZL, ProButtons.ZR]);
        if (_joyCons is { Count: 2 } parts && parts.All(p => !p.Kind.IsSwitch2()))
            keys.RemoveAll(b => b is ProButtons.C or ProButtons.GL or ProButtons.GR);
        if (_joyCons is { Count: 2 } pair && !pair.All(p => p.InGrip))
            keys.RemoveAll(b => b is ProButtons.GL or ProButtons.GR);
        var result = new List<ButtonIndicator>();
        foreach (var key in keys.Distinct())
        {
            float? level = (kind, key) switch
            {
                (ControllerKind.GameCube2, ProButtons.L) => _input?.LeftTrigger,
                (ControllerKind.GameCube2, ProButtons.R) => _input?.RightTrigger,
                (_, ProButtons.ZL) when kind is ControllerKind.XboxController or ControllerKind.DualShock4 or ControllerKind.DualSense => _input?.LeftTrigger,
                (_, ProButtons.ZR) when kind is ControllerKind.XboxController or ControllerKind.DualShock4 or ControllerKind.DualSense => _input?.RightTrigger,
                _ => null,
            };
            result.Add(new ButtonIndicator(ButtonLabel(key, upright ? ControllerKind.Pro2 : labelKind), _input?.Has(key) == true || level > 0.02f, level));
        }
        if (kind == ControllerKind.N64Controller)
        {
            result.Add(new("C ↑", _input?.RightY > 0.5f));
            result.Add(new("C ↓", _input?.RightY < -0.5f));
            result.Add(new("C ←", _input?.RightX < -0.5f));
            result.Add(new("C →", _input?.RightX > 0.5f));
        }
        return result;
    }

    private static string ButtonLabel(ProButtons key, ControllerKind kind)
    {
        if (kind == ControllerKind.GameCube2 && key == ProButtons.Plus) return "START";
        if (kind == ControllerKind.WiiRemote)
        {
            var symbol = key switch { ProButtons.L => "C", ProButtons.ZL => "Z", ProButtons.ZR => "B", _ => null };
            if (symbol is not null) return symbol;
        }
        if (kind == ControllerKind.XboxController)
        {
            var symbol = key switch { ProButtons.Minus => "▤", ProButtons.Plus => "☰", ProButtons.Home => "Xbox", _ => null };
            if (symbol is not null) return symbol;
        }
        if (kind.IsPlayStation())
        {
            var symbol = key switch { ProButtons.A => "○", ProButtons.B => "✕", ProButtons.X => "△", ProButtons.Y => "□", _ => null };
            if (symbol is not null) return symbol;
        }
        return key switch
        {
            ProButtons.Up => "↑", ProButtons.Down => "↓", ProButtons.Left => "←", ProButtons.Right => "→",
            ProButtons.LeftStick => "L3", ProButtons.RightStick => "R3",
            ProButtons.SLLeft or ProButtons.SLRight => "SL", ProButtons.SRLeft or ProButtons.SRRight => "SR",
            ProButtons.Capture => kind.IsPlayStation() ? Tr.T("Touchpad-Klick") : Tr.T("Aufnahme"),
            _ => Tr.T(ControllerButtons.Label(key, kind).Split('(')[0].Trim()),
        };
    }

    private static int GridColumns(int width) => Math.Clamp((width + UiScale.Px(6)) / UiScale.Px(78), 1, 8);

    internal int ButtonGridHeight(int width)
    {
        int rows = (ButtonIndicators().Count + GridColumns(width) - 1) / GridColumns(width);
        return Math.Max(UiScale.Px(46), rows * UiScale.Px(52) - UiScale.Px(6));
    }

    private void PaintButtonGrid(Graphics g)
    {
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var buttons = ButtonIndicators();
        int columns = GridColumns(Width), gap = UiScale.Px(6), height = UiScale.Px(46);
        float cell = (Width - (columns - 1) * gap) / (float)columns;
        using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter };
        for (int i = 0; i < buttons.Count; i++)
        {
            var button = buttons[i];
            var rect = new RectangleF((i % columns) * (cell + gap) + 1, (i / columns) * (height + gap) + 1, cell - 2, height - 2);
            using var path = Rounded(rect, UiScale.Px(7));
            Theme.PaintButton(g, path, rect, active: button.Active);
            var label = button.Label;
            if (button.Level is { } level) label += $"\n{Math.Clamp(level, 0, 1) * 100:0} %";
            using var text = new SolidBrush(Theme.ButtonText(button.Active));
            g.DrawString(label, UiFonts.Small, text, RectangleF.Inflate(rect, -UiScale.Px(3), -UiScale.Px(2)), format);
        }
    }
}
