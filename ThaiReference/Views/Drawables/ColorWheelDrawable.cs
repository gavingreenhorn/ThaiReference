using Microsoft.Maui.Graphics;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.Views.Drawables;

public class ColorWheelDrawable : IDrawable
{
    private IReadOnlyList<IHueFamily> _families = [];
    private float _cx, _cy, _r;

    public IHueFamily? SelectedFamily { get; set; }
    public IColor? SelectedColor { get; set; }
    public Color SelectionStrokeColor { get; set; } = Colors.White;
    public Color BackdropColor { get; set; } = Color.FromArgb("#1a1a2e");
    public Color CenterColor { get; set; } = Color.FromArgb("#252540");
    public Color TextColor { get; set; } = Color.FromArgb("#888888");

    private const float INNER_RADIUS_RATIO = 0.36f;
    private const float OUTER_RADIUS_RATIO = 0.98f;

    public void Configure(IReadOnlyList<IHueFamily> families)
    {
        _families = families;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        _cx = dirtyRect.Center.X;
        _cy = dirtyRect.Center.Y;
        _r = Math.Min(dirtyRect.Width, dirtyRect.Height) / 2f;
        canvas.Antialias = true;

        float innerRadius = _r * INNER_RADIUS_RATIO;
        float outerRadius = _r * OUTER_RADIUS_RATIO;

        canvas.FillColor = BackdropColor;
        canvas.FillCircle(_cx, _cy, _r);

        if (SelectedFamily == null)
        {
            DrawFamilyWheel(canvas, innerRadius, outerRadius);
            DrawEmptyCenter(canvas, innerRadius);
        }
        else
        {
            DrawColorsWheel(canvas, innerRadius, outerRadius);
            DrawFamilyCenter(canvas, innerRadius);
        }
    }

    private void DrawFamilyWheel(ICanvas canvas, float innerR, float outerR)
    {
        if (_families.Count == 0) return;

        float sweepPerFamily = 360f / _families.Count;
        float gap = 1.5f;

        for (int i = 0; i < _families.Count; i++)
        {
            var family = _families[i];
            float startDeg = i * sweepPerFamily - 90f + gap / 2f;
            float sweepDeg = sweepPerFamily - gap;

            var color = Color.FromArgb(family.Hex);
            DrawArc(canvas, outerR, innerR + 2f, startDeg, sweepDeg, color, false);

            float midDeg = startDeg + sweepDeg / 2f;
            float labelR = (innerR + outerR) / 2f;
            float lx = _cx + labelR * MathF.Cos(Rad(midDeg));
            float ly = _cy + labelR * MathF.Sin(Rad(midDeg));

            var textColor = IsLight(family.Hex) ? Colors.Black : Colors.White;
            canvas.FontColor = textColor;
            canvas.FontSize = _r * 0.07f;
            DrawCenteredLabel(canvas, family.ThaiName, lx, ly, _r * 0.07f);
        }
    }

    private void DrawColorsWheel(ICanvas canvas, float innerR, float outerR)
    {
        var colors = SelectedFamily?.Colors.OrderByDescending(c => GetLuminance(c.Hex)).ToList() ?? [];
        if (colors.Count == 0) return;

        float sweepPerColor = 360f / colors.Count;
        float gap = 0.8f;

        for (int i = 0; i < colors.Count; i++)
        {
            var color = colors[i];
            float startDeg = i * sweepPerColor - 90f + gap / 2f;
            float sweepDeg = sweepPerColor - gap;

            var fillColor = Color.FromArgb(color.Hex);
            bool active = SelectedColor?.Id == color.Id;
            DrawArc(canvas, outerR, innerR + 4f, startDeg, sweepDeg, fillColor, active);
        }
    }

    private void DrawEmptyCenter(ICanvas canvas, float radius)
    {
        canvas.FillColor = CenterColor;
        canvas.FillCircle(_cx, _cy, radius);

        canvas.FontColor = TextColor;
        canvas.FontSize = _r * 0.11f;
        DrawCenteredLabel(canvas, "สีไทย", _cx, _cy, _r * 0.11f);
    }

    private void DrawFamilyCenter(ICanvas canvas, float radius)
    {
        if (SelectedFamily == null) return;

        var familyColor = Color.FromArgb(SelectedFamily.Hex);
        canvas.FillColor = familyColor;
        canvas.FillCircle(_cx, _cy, radius);

        var textColor = IsLight(SelectedFamily.Hex) ? Colors.Black : Colors.White;
        canvas.FontColor = textColor;
        canvas.FontSize = _r * 0.11f;
        DrawCenteredLabel(canvas, SelectedFamily.ThaiName, _cx, _cy - _r * 0.03f, _r * 0.11f);

        canvas.FontSize = _r * 0.06f;
        DrawCenteredLabel(canvas, SelectedFamily.EnglishName, _cx, _cy + _r * 0.05f, _r * 0.06f);
    }

    private void DrawArc(ICanvas canvas, float rOut, float rIn, float startDeg, float sweepDeg, Color fill, bool active)
    {
        var path = BuildArcPath(rOut, rIn, startDeg, sweepDeg);

        canvas.FillColor = fill;
        canvas.FillPath(path);

        canvas.StrokeColor = BackdropColor;
        canvas.StrokeSize = 1f;
        canvas.DrawPath(path);

        if (active)
        {
            var insetPath = BuildArcPath(rOut - 1.5f, rIn + 1.5f, startDeg + 0.3f, sweepDeg - 0.6f);
            canvas.StrokeColor = SelectionStrokeColor;
            canvas.StrokeSize = 1.5f;
            canvas.DrawPath(insetPath);
        }
    }

    private PathF BuildArcPath(float rOut, float rIn, float startDeg, float sweepDeg)
    {
        float s1 = Rad(startDeg);
        float s2 = Rad(startDeg + sweepDeg);
        int steps = Math.Max(2, (int)(sweepDeg / 3));

        var path = new PathF();
        path.MoveTo(_cx + rOut * MathF.Cos(s1), _cy + rOut * MathF.Sin(s1));
        for (int i = 1; i <= steps; i++)
        {
            float a = s1 + (s2 - s1) * i / steps;
            path.LineTo(_cx + rOut * MathF.Cos(a), _cy + rOut * MathF.Sin(a));
        }
        path.LineTo(_cx + rIn * MathF.Cos(s2), _cy + rIn * MathF.Sin(s2));
        for (int i = steps - 1; i >= 0; i--)
        {
            float a = s1 + (s2 - s1) * i / steps;
            path.LineTo(_cx + rIn * MathF.Cos(a), _cy + rIn * MathF.Sin(a));
        }
        path.Close();
        return path;
    }

    private static void DrawCenteredLabel(ICanvas canvas, string text, float x, float y, float fontSize)
    {
        float w = fontSize * Math.Max(6f, text.Length * 1.2f);
        float h = fontSize * 1.6f;
        canvas.DrawString(
            text,
            x - w / 2f,
            y - h / 2f,
            w,
            h,
            HorizontalAlignment.Center,
            VerticalAlignment.Center,
            TextFlow.OverflowBounds);
    }

    private static float Rad(float deg) => deg * MathF.PI / 180f;

    private static bool IsLight(string hex)
    {
        if (string.IsNullOrEmpty(hex) || hex.Length < 7) return false;
        int r = Convert.ToInt32(hex.Substring(1, 2), 16);
        int g = Convert.ToInt32(hex.Substring(3, 2), 16);
        int b = Convert.ToInt32(hex.Substring(5, 2), 16);
        return (r * 299 + g * 587 + b * 114) / 1000 > 128;
    }

    private static float GetLuminance(string hex)
    {
        if (string.IsNullOrEmpty(hex) || hex.Length < 7) return 0f;
        float r = Convert.ToInt32(hex.Substring(1, 2), 16) / 255f;
        float g = Convert.ToInt32(hex.Substring(3, 2), 16) / 255f;
        float b = Convert.ToInt32(hex.Substring(5, 2), 16) / 255f;
        return 0.2126f * r + 0.7152f * g + 0.0722f * b;
    }

    public HitResult HitTest(float x, float y)
    {
        float dx = x - _cx;
        float dy = y - _cy;
        float dist = MathF.Sqrt(dx * dx + dy * dy);

        float innerRadius = _r * INNER_RADIUS_RATIO;
        float outerRadius = _r * OUTER_RADIUS_RATIO;

        if (dist > outerRadius) return HitResult.None;

        if (dist <= innerRadius)
        {
            return new HitResult(HitType.Center, null, null);
        }

        float angleDeg = MathF.Atan2(dy, dx) * 180f / MathF.PI;
        angleDeg += 90f;
        if (angleDeg < 0) angleDeg += 360f;
        if (angleDeg >= 360f) angleDeg -= 360f;

        if (SelectedFamily == null)
        {
            if (_families.Count == 0) return HitResult.None;
            float sweepPerFamily = 360f / _families.Count;
            int familyIndex = (int)(angleDeg / sweepPerFamily);
            familyIndex = Math.Clamp(familyIndex, 0, _families.Count - 1);
            return new HitResult(HitType.Family, _families[familyIndex], null);
        }
        else
        {
            var colors = SelectedFamily.Colors.OrderByDescending(c => GetLuminance(c.Hex)).ToList();
            if (colors.Count == 0) return HitResult.None;
            float sweepPerColor = 360f / colors.Count;
            int colorIndex = (int)(angleDeg / sweepPerColor);
            colorIndex = Math.Clamp(colorIndex, 0, colors.Count - 1);
            return new HitResult(HitType.Color, SelectedFamily, colors[colorIndex]);
        }
    }

    public enum HitType { None, Center, Family, Color }

    public record HitResult(HitType Type, IHueFamily? Family, IColor? Color)
    {
        public static readonly HitResult None = new(HitType.None, null, null);
        public bool IsNone => Type == HitType.None;
    }
}
