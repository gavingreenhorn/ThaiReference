using Microsoft.Maui.Graphics;
using ThaiReference.Models.Interfaces;
using ThaiReference.Utilities;
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
        public int SelectedSeasonOrdinal { get; set; } = -1;

        public readonly record struct Hit(bool IsSeason, int Ordinal)
        {
            public static Hit None => new(false, -1);
            public bool IsNone => Ordinal < 0;
        }

        private const float SEASON_OUT = 0.97f;
        private const float SEASON_IN = 0.85f;
        private const float OUTER_OUT = 0.835f;
        private const float OUTER_IN = 0.67f;
        private const float INNER_OUT = 0.63f;
        private const float INNER_IN = 0.30f;

        private float _cx, _cy, _r;

        private static float SegStart(float stepDeg, int i) => i * stepDeg - 90f;
        private static float Rad(float deg) => deg * MathF.PI / 180f;

        private List<Slot> _slots = [];
        private List<SeasonSlot> _seasonSlots = [];

        public void Configure(IEnumerable<ISeason> seasons, IEnumerable<MonthWheelItemVm> months)
        {
            _slots = (months ?? [])
                .OrderBy(m => m.Ordinal)
                .Select(m => new Slot(
                    m.Ordinal,
                    m.ShortName,
                    m.ShortEnglish,
                    m.ZodiacSymbol,
                    Color.FromArgb(m.ElementLightHex),
                    Color.FromArgb(m.ElementDarkHex),
                    m.SeasonOrdinal))
                .ToList();

            _seasonSlots = (seasons ?? []).Select(BuildSeasonSlot).Where(s => s is not null).Select(s => s!).ToList();
        }

        // Each season arc spans its member months. Its first slot is the member whose predecessor
        // on the wheel is not a member, so a season spanning Dec..Jan stays one contiguous arc.
        private SeasonSlot? BuildSeasonSlot(ISeason season)
        {
            var members = season.Months.Select(m => m.Ordinal).ToHashSet();
            int n = _slots.Count;
            if (members.Count == 0 || n == 0) return null;

            int start = Enumerable.Range(0, n).FirstOrDefault(
                i => members.Contains(_slots[i].Ordinal) && !members.Contains(_slots[(i - 1 + n) % n].Ordinal),
                _slots.FindIndex(s => members.Contains(s.Ordinal)));
            if (start < 0) return null;

            string label = string.IsNullOrEmpty(season.Glyph) ? season.Spelling : season.Glyph + "\uFE0E";
            string key = "Season" + season.EnglishLabel;
            return new SeasonSlot(season.Ordinal, label, ThemeColors.Get(key), ThemeColors.Get(key + "Glyph"), start, members.Count);
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            _cx = dirtyRect.Center.X;
            _cy = dirtyRect.Center.Y;
            _r = Math.Min(dirtyRect.Width, dirtyRect.Height) / 2f;
            canvas.Antialias = true;

            canvas.FillColor = BackdropColor;
            canvas.FillCircle(_cx, _cy, _r * SEASON_OUT + 4);

            DrawSeasonRing(canvas);
            DrawZodiacRing(canvas);
            DrawInnerRing(canvas);

            canvas.FillColor = CenterFillColor;
            canvas.FillCircle(_cx, _cy, _r * INNER_IN - 2);
            canvas.StrokeColor = CenterStrokeColor;
            canvas.StrokeSize = 1.5f;
            canvas.DrawCircle(_cx, _cy, _r * INNER_IN - 2);
        }

        private void DrawSeasonRing(ICanvas canvas)
        {
            if (_seasonSlots.Count == 0 || _slots.Count == 0) return;

            const float GAP = 1.5f;
            float stepDeg = 360f / _slots.Count;

            int selectedSeasonOrdinal = SelectedSeasonOrdinal >= 0
                ? SelectedSeasonOrdinal
                : _slots.FirstOrDefault(s => s.Ordinal == SelectedOrdinal)?.SeasonOrdinal ?? -1;

            for (int i = 0; i < _seasonSlots.Count; i++)
            {
                var s = _seasonSlots[i];
                bool active = selectedSeasonOrdinal == s.SeasonOrdinal;

                float startDeg = SegStart(stepDeg, s.StartIndex) + GAP / 2f;
                float sweepDeg = stepDeg * s.Count - GAP;

                DrawArc(canvas, _r * SEASON_OUT, _r * SEASON_IN, startDeg, sweepDeg,
                    // active ? s.Color.WithLuminosity(Math.Min(s.Color.GetLuminosity() + 0.1f, 1f)) : s.Color,
                    s.Color,
                    active);

                float midDeg = startDeg + sweepDeg / 2f;
                float midR = _r * (SEASON_OUT + SEASON_IN) / 2f;
                float lx = _cx + midR * MathF.Cos(Rad(midDeg));
                float ly = _cy + midR * MathF.Sin(Rad(midDeg));

                canvas.FontColor = s.GlyphColor;
                canvas.FontSize = _r * 0.08f;
                canvas.DrawString(s.Label, lx, ly + _r * 0.025f, HorizontalAlignment.Center);
            }
        }

        private void DrawZodiacRing(ICanvas canvas)
        {
            if (_slots.Count == 0) return;

            const float GAP = 1.5f;
            float stepDeg = 360f / _slots.Count;
            for (int i = 0; i < _slots.Count; i++)
            {
                var s = _slots[i];
                bool active = SelectedOrdinal == s.Ordinal;

                float startDeg = SegStart(stepDeg, i) + GAP / 2f;
                float sweepDeg = stepDeg - GAP;

                DrawArc(canvas, _r * OUTER_OUT, _r * OUTER_IN, startDeg, sweepDeg,
                    active ? s.DarkColor.WithLuminosity(Math.Min(s.DarkColor.GetLuminosity() + 0.1f, 1f)) : s.DarkColor,
                    active);

                float midDeg = startDeg + sweepDeg / 2f;
                float midR = _r * (OUTER_OUT + OUTER_IN) / 2f;
                float lx = _cx + midR * MathF.Cos(Rad(midDeg));
                float ly = _cy + midR * MathF.Sin(Rad(midDeg));

                canvas.FontColor = ZodiacSymbolColor;
                canvas.FontSize = _r * 0.085f;
                canvas.DrawString(s.ZodiacSymbol, lx, ly, HorizontalAlignment.Center);
            }
        }

        private void DrawInnerRing(ICanvas canvas)
        {
            if (_slots.Count == 0) return;

            const float GAP = 1.5f;
            float stepDeg = 360f / _slots.Count;
            for (int i = 0; i < _slots.Count; i++)
            {
                var s = _slots[i];
                bool active = SelectedOrdinal == s.Ordinal;

                float startDeg = SegStart(stepDeg, i) + GAP / 2f;
                float sweepDeg = stepDeg - GAP;

                DrawArc(canvas, _r * INNER_OUT, _r * INNER_IN, startDeg, sweepDeg,
                    active ? s.LightColor.WithLuminosity(Math.Min(s.LightColor.GetLuminosity() + 0.08f, 1f)) : s.LightColor,
                    active);

                float midDeg = startDeg + sweepDeg / 2f;
                float midR = _r * (INNER_OUT + INNER_IN) / 2f;
                float lx = _cx + midR * MathF.Cos(Rad(midDeg));
                float ly = _cy + midR * MathF.Sin(Rad(midDeg));

                canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
                canvas.FontColor = Color.FromRgba(0, 0, 0, 255);
                canvas.FontSize = _r * 0.065f;
                canvas.DrawString(s.ShortName, lx, ly - _r * 0.03f, HorizontalAlignment.Center);

                canvas.Font = Microsoft.Maui.Graphics.Font.Default;
                canvas.FontColor = Color.FromRgba(0, 0, 0, 200);
                canvas.FontSize = _r * 0.05f;
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

        /// <summary>Returns the tapped season (outer ring) or month (inner ring).</summary>
        public Hit HitTest(float x, float y)
        {
            if (_slots.Count == 0) return Hit.None;

            float dx = x - _cx;
            float dy = y - _cy;
            float dist = MathF.Sqrt(dx * dx + dy * dy);

            if (dist > _r * SEASON_OUT || dist < _r * INNER_IN) return Hit.None;
            bool isSeason = dist >= _r * SEASON_IN;

            float angleDeg = MathF.Atan2(dy, dx) * 180f / MathF.PI + 90f;
            if (angleDeg < 0f) angleDeg += 360f;
            if (angleDeg >= 360f) angleDeg -= 360f;

            // Month arcs are laid out by list index, so derive the step from the slot count
            // instead of assuming a full 12-month wheel.
            float stepDeg = 360f / _slots.Count;
            int idx = (int)(angleDeg / stepDeg);
            if (idx < 0 || idx >= _slots.Count) return Hit.None;
            return isSeason
                ? new Hit(true, _slots[idx].SeasonOrdinal)
                : new Hit(false, _slots[idx].Ordinal);
        }

        private record Slot(
            int Ordinal,
            string ShortName,
            string ShortEnglish,
            string ZodiacSymbol,
            Color LightColor,
            Color DarkColor,
            int SeasonOrdinal);

        private record SeasonSlot(
            int SeasonOrdinal,
            string Label,
            Color Color,
            Color GlyphColor,
            int StartIndex,
            int Count);
    }
}
