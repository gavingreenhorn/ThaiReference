using Microsoft.Maui.Graphics;
using ThaiReference.ViewModels;

namespace ThaiReference.Views.Drawables
{
    public class ThaiMonthsDrawable : IDrawable
    {
        public Color BackdropColor { get; set; } = Colors.Black;
        public Color CenterFillColor { get; set; } = Colors.Black;
        public Color CenterStrokeColor { get; set; } = Colors.Gray;
        public Color ZodiacSymbolColor { get; set; } = Colors.PaleGoldenrod;
        public Color SelectionStrokeColor { get; set; } = Colors.Gold;

        public int SelectedOrdinal { get; set; } = -1;

        private const float OUTER_OUT = 0.97f;
        private const float OUTER_IN = 0.68f;
        private const float INNER_OUT = 0.63f;
        private const float INNER_IN = 0.30f;

        private float _cx, _cy, _r;

        private static float SegStart(int i) => i * 30f - 90f;
        private static float Rad(float deg) => deg * MathF.PI / 180f;

        private List<Slot> _slots = [];

        public void Configure(IEnumerable<MonthWheelItemVm> months)
        {
            _slots = (months ?? [])
                .OrderBy(m => m.Ordinal)
                .Select(m => new Slot(
                    m.Ordinal,
                    m.ShortName,
                    m.ShortEnglish,
                    m.ZodiacSymbol,
                    Color.FromArgb(m.ElementLightHex),
                    Color.FromArgb(m.ElementDarkHex)))
                .ToList();
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            _cx = dirtyRect.Center.X;
            _cy = dirtyRect.Center.Y;
            _r = Math.Min(dirtyRect.Width, dirtyRect.Height) / 2f;
            canvas.Antialias = true;

            canvas.FillColor = BackdropColor;
            canvas.FillCircle(_cx, _cy, _r * OUTER_OUT + 4);

            DrawOuterRing(canvas);
            DrawInnerRing(canvas);

            canvas.FillColor = CenterFillColor;
            canvas.FillCircle(_cx, _cy, _r * INNER_IN - 2);
            canvas.StrokeColor = CenterStrokeColor;
            canvas.StrokeSize = 1.5f;
            canvas.DrawCircle(_cx, _cy, _r * INNER_IN - 2);
        }

        private void DrawOuterRing(ICanvas canvas)
        {
            const float GAP = 1.5f;
            for (int i = 0; i < _slots.Count; i++)
            {
                var s = _slots[i];
                bool active = SelectedOrdinal == s.Ordinal;

                float startDeg = SegStart(i) + GAP / 2f;
                float sweepDeg = 30f - GAP;

                DrawArc(canvas, _r * OUTER_OUT, _r * OUTER_IN, startDeg, sweepDeg,
                    active ? s.DarkColor.WithLuminosity(Math.Min(s.DarkColor.GetLuminosity() + 0.1f, 1f)) : s.DarkColor,
                    active);

                float midDeg = startDeg + sweepDeg / 2f;
                float midR = _r * (OUTER_OUT + OUTER_IN) / 2f;
                float lx = _cx + midR * MathF.Cos(Rad(midDeg));
                float ly = _cy + midR * MathF.Sin(Rad(midDeg));

                canvas.FontColor = ZodiacSymbolColor;
                canvas.FontSize = _r * 0.1f;
                canvas.DrawString(s.ZodiacSymbol, lx, ly, HorizontalAlignment.Center);
            }
        }

        private void DrawInnerRing(ICanvas canvas)
        {
            const float GAP = 1.5f;
            for (int i = 0; i < _slots.Count; i++)
            {
                var s = _slots[i];
                bool active = SelectedOrdinal == s.Ordinal;

                float startDeg = SegStart(i) + GAP / 2f;
                float sweepDeg = 30f - GAP;

                DrawArc(canvas, _r * INNER_OUT, _r * INNER_IN, startDeg, sweepDeg,
                    active ? s.LightColor.WithLuminosity(Math.Min(s.LightColor.GetLuminosity() + 0.08f, 1f)) : s.LightColor,
                    active);

                float midDeg = startDeg + sweepDeg / 2f;
                float midR = _r * (INNER_OUT + INNER_IN) / 2f;
                float lx = _cx + midR * MathF.Cos(Rad(midDeg));
                float ly = _cy + midR * MathF.Sin(Rad(midDeg));

                canvas.FontColor = Color.FromRgba(0, 0, 0, active ? 230 : 200);
                canvas.FontSize = _r * 0.055f;
                canvas.DrawString(s.ShortName, lx, ly - _r * 0.03f, HorizontalAlignment.Center);

                canvas.FontColor = Color.FromRgba(0, 0, 0, 140);
                canvas.FontSize = _r * 0.045f;
                canvas.DrawString(s.ShortEnglish, lx, ly + _r * 0.045f, HorizontalAlignment.Center);
            }
        }

        private void DrawArc(ICanvas canvas, float rOut, float rIn, float startDeg, float sweepDeg, Color fill, bool active)
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

            canvas.FillColor = fill;
            canvas.FillPath(path);

            if (active)
            {
                canvas.StrokeColor = SelectionStrokeColor;
                canvas.StrokeSize = 1.5f;
                canvas.DrawPath(path);
            }
        }

        /// <summary>Returns the tapped month's ordinal, or -1 if outside the wheel.</summary>
        public int HitTest(float x, float y)
        {
            float dx = x - _cx;
            float dy = y - _cy;
            float dist = MathF.Sqrt(dx * dx + dy * dy);

            if (dist > _r * OUTER_OUT || dist < _r * INNER_IN) return -1;

            float angleDeg = MathF.Atan2(dy, dx) * 180f / MathF.PI + 90f;
            if (angleDeg < 0f) angleDeg += 360f;
            if (angleDeg >= 360f) angleDeg -= 360f;

            int idx = (int)(angleDeg / 30f) % 12;
            return idx >= 0 && idx < _slots.Count ? _slots[idx].Ordinal : -1;
        }

        private record Slot(
            int Ordinal,
            string ShortName,
            string ShortEnglish,
            string ZodiacSymbol,
            Color LightColor,
            Color DarkColor);
    }
}
