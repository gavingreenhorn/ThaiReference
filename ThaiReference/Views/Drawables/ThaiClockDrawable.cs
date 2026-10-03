using Microsoft.Maui.Graphics;
using ThaiReference.ViewModels;

namespace ThaiReference.Views.Drawables
{
    // 06:00 = right (0°), 12:00 = bottom (90°), 18:00 = left (180°), 00:00 = top (-90°)
    public class ThaiClockDrawable : IDrawable
    {
        public int SelectedOuterOrdinal { get; set; } = -1;
        public int SelectedInnerHour { get; set; } = -1;
        public int SelectedDayNightSegment { get; set; } = -1;
        public bool IsCenterSelected { get; set; } = true;

        public Color BackdropColor { get; set; } = Colors.Black;
        public Color CenterFillColor { get; set; } = Colors.Black;
        public Color CenterStrokeColor { get; set; } = Colors.Gray;
        public Color SelectionStrokeColor { get; set; } = Colors.Gold;
        public Color DayColor { get; set; } = Colors.SteelBlue;
        public Color NightColor { get; set; } = Colors.DarkSlateBlue;
        public Color InnerFallbackColor { get; set; } = Colors.DimGray;

        private const float DAYNIGHT_OUT = 0.97f;
        private const float DAYNIGHT_IN = 0.86f;
        private const float OUTER_OUT = 0.82f;
        private const float OUTER_IN = 0.57f;
        private const float INNER_OUT = 0.53f;
        private const float INNER_IN = 0.34f;

        private float _cx, _cy, _r;

        private static float SegStart(int h) => h * 15f - 90f - 7.5f;
        private static float SegCenter(int h) => h * 15f - 90f;
        private static float Rad(float deg) => deg * MathF.PI / 180f;

        private List<Segment> _segments = [];

        public void Configure(IEnumerable<ClockStripSegmentVm> segments)
        {
            _segments = (segments ?? [])
                .Select(s => new Segment(
                    s.Ordinal, s.Thai, s.StartHour, s.EndHour, Color.FromArgb(s.ColorHex)))
                .OrderBy(s => s.Ordinal)
                .ToList();
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            _cx = dirtyRect.Center.X;
            _cy = dirtyRect.Center.Y;
            _r = Math.Min(dirtyRect.Width, dirtyRect.Height) / 2f;
            canvas.Antialias = true;

            canvas.FillColor = BackdropColor;
            canvas.FillCircle(_cx, _cy, _r * DAYNIGHT_OUT + 4);

            DrawDayNightRing(canvas);
            DrawOuterRing(canvas);
            DrawInnerRing(canvas);
            DrawCenter(canvas);
        }

        private void DrawCenter(ICanvas canvas)
        {
            float centerR = _r * INNER_IN - 2f;

            canvas.FillColor = CenterFillColor;
            canvas.FillCircle(_cx, _cy, centerR);

            canvas.StrokeColor = IsCenterSelected ? SelectionStrokeColor : CenterStrokeColor;
            canvas.StrokeSize = 1.5f;
            canvas.DrawCircle(_cx, _cy, centerR);
            // LCD digits are Labels overlaid in ThaiClockView; hub fill is the ink panel color.
        }

        private void DrawDayNightRing(ICanvas canvas)
        {
            DrawArc(canvas, _r * DAYNIGHT_OUT, _r * DAYNIGHT_IN, 0f, 180f, DayColor, SelectedDayNightSegment == 0);
            DrawArc(canvas, _r * DAYNIGHT_OUT, _r * DAYNIGHT_IN, 180f, 180f, NightColor, SelectedDayNightSegment == 1);

            float midR = _r * (DAYNIGHT_OUT + DAYNIGHT_IN) / 2f;
            canvas.FontColor = Colors.White.WithAlpha(0.85f);
            float dayNightFont = _r * 0.065f;
            canvas.FontSize = dayNightFont;
            DrawCenteredLabel(canvas, "กลางวัน", _cx + midR * MathF.Cos(Rad(90f)), _cy + midR * MathF.Sin(Rad(90f)), dayNightFont);
            DrawCenteredLabel(canvas, "กลางคืน", _cx + midR * MathF.Cos(Rad(-90f)), _cy + midR * MathF.Sin(Rad(-90f)), dayNightFont);
        }

        private void DrawOuterRing(ICanvas canvas)
        {
            const float GAP = 0.8f;
            foreach (var s in _segments)
            {
                bool active = SelectedOuterOrdinal == s.Ordinal;

                float startDeg = SegStart(s.StartHour) + GAP / 2f;
                float endDeg = SegStart(s.EndHour + 1) - GAP / 2f;
                float sweepDeg = endDeg - startDeg;
                if (sweepDeg < 0) sweepDeg += 360f;

                var color = active
                    ? s.Color.WithLuminosity(Math.Min(s.Color.GetLuminosity() + 0.12f, 1f))
                    : s.Color;

                DrawArc(canvas, _r * OUTER_OUT, _r * OUTER_IN, startDeg, sweepDeg, color, active);

                float midDeg = startDeg + sweepDeg / 2f;
                float midR = _r * (OUTER_OUT + OUTER_IN) / 2f;
                float lx = _cx + midR * MathF.Cos(Rad(midDeg));
                float ly = _cy + midR * MathF.Sin(Rad(midDeg));

                // Midnight / noon: sun/moon glyphs fit the thin arcs; other bands keep Thai.
                string label = OuterRingLabel(s);
                float fontSize = _r * (label.Length <= 2 ? 0.12f : 0.095f);
                canvas.FontColor = Colors.White;
                canvas.FontSize = fontSize;
                DrawCenteredLabel(canvas, label, lx, ly, fontSize);
            }
        }

        private static string OuterRingLabel(Segment s) =>
            s.StartHour == 0 && s.EndHour == 0 ? "☽"
            : s.StartHour == 12 && s.EndHour == 12 ? "☀"
            : s.Thai;

        /// <summary>
        /// Point (x,y) is the geometric center of the band; the 2-arg DrawString overload
        /// anchors on the baseline so glyphs sit high — use a box + VerticalAlignment.Center.
        /// </summary>
        private static void DrawCenteredLabel(ICanvas canvas, string text, float x, float y, float fontSize)
        {
            // Wide enough that Thai labels stay on one line; height only needs to cover the glyph.
            float w = fontSize * Math.Max(4f, text.Length * 1.1f);
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

        private void DrawInnerRing(ICanvas canvas)
        {
            const float GAP = 0.6f;
            Segment? band = SelectedOuterOrdinal >= 0
                ? _segments.FirstOrDefault(s => s.Ordinal == SelectedOuterOrdinal)
                : null;

            for (int h = 0; h < 24; h++)
            {
                bool hourActive = SelectedInnerHour == h;
                bool inBand = band is not null && HourInSegment(h, band);
                float startDeg = SegStart(h) + GAP / 2f;
                float sweepDeg = 15f - GAP;

                var seg = SegmentForHour(h);
                var color = seg?.Color.WithAlpha(hourActive || inBand ? 1f : 0.7f) ?? InnerFallbackColor;
                if (hourActive || inBand)
                    color = color.WithLuminosity(Math.Min(color.GetLuminosity() + 0.15f, 1f));

                // Per-hour gold stroke only for a single selected hour — not for band selection.
                DrawArc(canvas, _r * INNER_OUT, _r * INNER_IN, startDeg, sweepDeg, color, hourActive);

                float midR = _r * (INNER_OUT + INNER_IN) / 2f;
                float cx = _cx + midR * MathF.Cos(Rad(SegCenter(h)));
                float cy = _cy + midR * MathF.Sin(Rad(SegCenter(h)));
                canvas.FontColor = Color.FromRgba(255, 255, 255, hourActive || inBand ? 255 : 200);
                float hourFont = _r * 0.058f;
                canvas.FontSize = hourFont;
                DrawCenteredLabel(canvas, h.ToString(), cx, cy, hourFont);
            }

            // One outline around all hours belonging to the selected time-of-day.
            if (band is not null)
            {
                float startDeg = SegStart(band.StartHour) + GAP / 2f;
                float endDeg = SegStart(band.EndHour + 1) - GAP / 2f;
                float sweepDeg = endDeg - startDeg;
                if (sweepDeg < 0) sweepDeg += 360f;
                StrokeArc(canvas, _r * INNER_OUT, _r * INNER_IN, startDeg, sweepDeg);
            }
        }

        private void DrawArc(ICanvas canvas, float rOut, float rIn, float startDeg, float sweepDeg, Color fill, bool active)
        {
            var path = BuildArcPath(rOut, rIn, startDeg, sweepDeg);

            canvas.FillColor = fill;
            canvas.FillPath(path);

            if (active)
                StrokePath(canvas, path);
        }

        private void StrokeArc(ICanvas canvas, float rOut, float rIn, float startDeg, float sweepDeg) =>
            StrokePath(canvas, BuildArcPath(rOut, rIn, startDeg, sweepDeg));

        private void StrokePath(ICanvas canvas, PathF path)
        {
            canvas.StrokeColor = SelectionStrokeColor;
            canvas.StrokeSize = 1.5f;
            canvas.DrawPath(path);
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

        public HitResult HitTest(float x, float y)
        {
            float dx = x - _cx;
            float dy = y - _cy;
            float dist = MathF.Sqrt(dx * dx + dy * dy);

            if (dist > _r * DAYNIGHT_OUT) return HitResult.None;

            float centerR = _r * INNER_IN - 2f;
            if (dist <= centerR)
                return new HitResult(HitRing.Center, 0);

            if (dist < _r * INNER_IN) return HitResult.None;

            float angleDeg = MathF.Atan2(dy, dx) * 180f / MathF.PI;
            if (angleDeg < 0) angleDeg += 360f;

            if (dist >= _r * DAYNIGHT_IN && dist <= _r * DAYNIGHT_OUT)
            {
                int segment = angleDeg < 180f ? 0 : 1;
                return new HitResult(HitRing.DayNight, segment);
            }

            float shifted = angleDeg + 90f + 7.5f;
            while (shifted >= 360f) shifted -= 360f;
            int h = (int)(shifted / 15f) % 24;

            if (dist >= _r * OUTER_IN && dist <= _r * OUTER_OUT)
            {
                var seg = SegmentForHour(h);
                if (seg is not null) return new HitResult(HitRing.Outer, seg.Ordinal);
            }

            if (dist >= _r * INNER_IN && dist <= _r * INNER_OUT)
                return new HitResult(HitRing.Inner, h);

            return HitResult.None;
        }

        private Segment? SegmentForHour(int h)
        {
            foreach (var s in _segments)
                if (HourInSegment(h, s))
                    return s;
            return null;
        }

        private static bool HourInSegment(int h, Segment s) =>
            s.EndHour >= s.StartHour
                ? h >= s.StartHour && h <= s.EndHour
                : h >= s.StartHour || h <= s.EndHour;

        private record Segment(int Ordinal, string Thai, int StartHour, int EndHour, Color Color);

        public enum HitRing { Center, DayNight, Outer, Inner }

        public record HitResult(HitRing Ring, int Index)
        {
            public static readonly HitResult None = new(HitRing.Outer, -1);
            public bool IsNone => Index < 0;
        }
    }
}
