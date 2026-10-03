using Microsoft.Maui.Graphics;
using ThaiReference.Models;

namespace ThaiReference.Views.Drawables;

public class ClusterAnatomyDrawable : IDrawable
{
    public ClusterAnatomyType ClusterType { get; set; } = ClusterAnatomyType.True;

    private const float Pad = 12f;
    private const float Gap = 8f;
    private const float BoxH = 52f;
    private const float BoxW = 52f;
    private const float PlusW = 14f;
    private const float BracketDepth = 7f;
    private const int ThaiGlyphFontSize = 32;
    private const string ThaiFont = "NotoSerifThai";

    // C1 — pronounced initial consonant
    private static readonly Color ColC1 = Color.FromArgb("#0C447C");
    private static readonly Color BgC1 = Color.FromArgb("#E6F1FB");
    private static readonly Color StrkC1 = Color.FromArgb("#185FA5");

    // C2 — cluster / second consonant (pronounced)
    private static readonly Color ColC2 = Color.FromArgb("#3C3489");
    private static readonly Color BgC2 = Color.FromArgb("#EEEDFE");
    private static readonly Color StrkC2 = Color.FromArgb("#534AB7");

    // Silent — not pronounced (false C2, tone-shift ห/อ)
    private static readonly Color ColSilent = Color.FromArgb("#6B6960");
    private static readonly Color BgSilent = Color.FromArgb("#EEEDEA");
    private static readonly Color StrkSilent = Color.FromArgb("#B8B6AE");

    // V — vowel (main syllable vowel, epenthetic ะ, sub-syllable ◌ิ)
    private static readonly Color ColV = Color.FromArgb("#27500A");
    private static readonly Color BgV = Color.FromArgb("#EAF3DE");
    private static readonly Color StrkV = Color.FromArgb("#3B6D11");

    // Cf — optional final consonant
    private static readonly Color ColCf = Color.FromArgb("#633806");
    private static readonly Color BgCf = Color.FromArgb("#FAEEDA");
    private static readonly Color StrkCf = Color.FromArgb("#854F0B");

    private static readonly Color ColPlus = Color.FromArgb("#999990");
    private static readonly Color ColBracket = Color.FromArgb("#A8A6A0");

    public void Draw(ICanvas canvas, RectF rect)
    {
        canvas.Antialias = true;

        switch (ClusterType)
        {
            case ClusterAnatomyType.False:
                DrawFalse(canvas, rect);
                break;
            case ClusterAnatomyType.Leading:
                DrawLeading(canvas, rect);
                break;
            case ClusterAnatomyType.Enepenthetic:
                DrawEnepenthetic(canvas, rect);
                break;
            case ClusterAnatomyType.NonSonorant:
                DrawNonSonorant(canvas, rect);
                break;
            case ClusterAnatomyType.SubSyllable:
                DrawSubSyllable(canvas, rect);
                break;
            default:
                DrawTrue(canvas, rect);
                break;
        }
    }

    private static void DrawTrue(ICanvas canvas, RectF rect)
    {
        var row = BeginRow(rect, MeasureSyllableRow(consonantCount: 2));
        row = DrawActiveBox(row, canvas, "C1", "initial", BgC1, StrkC1, ColC1);
        row = DrawActiveBox(row, canvas, "C2", "cluster", BgC2, StrkC2, ColC2);
        DrawCouplingBracket(canvas, row.BracketFrom, row.BracketTo, row.BoxY + BoxH + 3f);
        DrawVowelSuffix(row, canvas);
    }

    private static void DrawFalse(ICanvas canvas, RectF rect)
    {
        var row = BeginRow(rect, MeasureSyllableRow(consonantCount: 2));
        row = DrawActiveBox(row, canvas, "C1", "initial", BgC1, StrkC1, ColC1);
        row = DrawSilentBox(row, canvas, "C2", "silent");
        DrawVowelSuffix(row, canvas);
    }

    private static void DrawLeading(ICanvas canvas, RectF rect)
    {
        var row = BeginRow(rect, MeasureSyllableRow(consonantCount: 2));
        row = DrawSilentBox(row, canvas, "ห/อ", "silent");
        row = DrawActiveBox(row, canvas, "C", "low-class", BgC1, StrkC1, ColC1);
        DrawVowelSuffix(row, canvas);
    }

    private static void DrawEnepenthetic(ICanvas canvas, RectF rect)
    {
        var row = BeginRow(rect, MeasureSyllableRow(consonantCount: 3));
        row = DrawActiveBox(row, canvas, "C1", "initial", BgC1, StrkC1, ColC1);
        row = DrawGlyphBox(row, canvas, "ะ", BgV, StrkV, ColV);
        row = DrawActiveBox(row, canvas, "C2", "cluster", BgC2, StrkC2, ColC2);
        DrawCouplingBracket(canvas, row.BracketFrom, row.BracketTo, row.BoxY + BoxH + 3f);
        DrawVowelSuffix(row, canvas);
    }

    private static void DrawNonSonorant(ICanvas canvas, RectF rect)
    {
        var row = BeginRow(rect, MeasureSyllableRow(consonantCount: 2));
        row = DrawActiveBox(row, canvas, "C1", "initial", BgC1, StrkC1, ColC1);
        row = DrawActiveBox(row, canvas, "C2", "non-sonorant", BgC2, StrkC2, ColC2, wrapSublabelOnHyphen: true);
        DrawCouplingBracket(canvas, row.BracketFrom, row.BracketTo, row.BoxY + BoxH + 3f);
        DrawVowelSuffix(row, canvas);
    }

    // Sub-syllable: บ + ร (cluster) + ◌ิ only — no V, no Cf, no + suffix.
    private static void DrawSubSyllable(ICanvas canvas, RectF rect)
    {
        float contentWidth = Pad * 2 + BoxW + Gap + BoxW + Gap + BoxW;
        var row = BeginRow(rect, contentWidth);
        row = DrawActiveBox(row, canvas, "บ", "C1", BgC1, StrkC1, ColC1);
        row = DrawActiveBox(row, canvas, "ร", "C2", BgC2, StrkC2, ColC2);
        DrawCouplingBracket(canvas, row.BracketFrom, row.BracketTo, row.BoxY + BoxH + 3f);
        DrawGlyphBox(row, canvas, "◌ิ", BgV, StrkV, ColV);
    }

    private static float MeasureSyllableRow(int consonantCount) =>
        Pad * 2
        + consonantCount * BoxW
        + (consonantCount - 1) * Gap
        + Gap + PlusW + Gap + BoxW + Gap + PlusW + Gap + BoxW;

    private readonly ref struct RowState
    {
        public float X { get; init; }
        public float BoxY { get; init; }
        public float BracketFrom { get; init; }
        public float BracketTo { get; init; }

        public RowState Advance(float delta = BoxW + Gap) =>
            new() { X = X + delta, BoxY = BoxY, BracketFrom = BracketFrom, BracketTo = BracketTo };

        public RowState WithBracketTo(float to) =>
            new() { X = X, BoxY = BoxY, BracketFrom = BracketFrom, BracketTo = to };
    }

    private static RowState BeginRow(RectF rect, float contentWidth)
    {
        float boxY = rect.Y + (rect.Height - BoxH) / 2f;
        float x = rect.X + Math.Max(Pad, (rect.Width - contentWidth) / 2f);
        return new RowState { X = x, BoxY = boxY, BracketFrom = x, BracketTo = x };
    }

    private static RowState DrawActiveBox(
        RowState row, ICanvas canvas,
        string label, string sublabel,
        Color bg, Color stroke, Color fg,
        bool wrapSublabelOnHyphen = false)
    {
        DrawBox(canvas, row.X, row.BoxY, BoxW, BoxH, label, sublabel, bg, stroke, fg, wrapSublabelOnHyphen);
        return row.WithBracketTo(row.X + BoxW).Advance();
    }

    private static RowState DrawSilentBox(RowState row, ICanvas canvas, string label, string sublabel)
    {
        DrawBox(canvas, row.X, row.BoxY, BoxW, BoxH, label, sublabel, BgSilent, StrkSilent, ColSilent);
        return row.Advance();
    }

    private static RowState DrawGlyphBox(RowState row, ICanvas canvas, string glyph, Color bg, Color stroke, Color fg)
    {
        DrawBoxShell(canvas, row.X, row.BoxY, BoxW, BoxH, bg, stroke);
        DrawThaiGlyphInBox(canvas, row.X, row.BoxY, BoxW, BoxH, glyph, fg);
        return row.Advance();
    }

    private static void DrawVowelSuffix(RowState row, ICanvas canvas)
    {
        float x = DrawPlus(canvas, row.X, row.BoxY);
        DrawBox(canvas, x, row.BoxY, BoxW, BoxH, "V", "vowel", BgV, StrkV, ColV);
        x += BoxW + Gap;
        x = DrawPlus(canvas, x, row.BoxY);
        DrawOptionalBox(canvas, x, row.BoxY, BoxW, BoxH, "Cf", "final", BgCf, StrkCf, ColCf);
    }

    private static float DrawPlus(ICanvas canvas, float x, float boxY)
    {
        DrawCenteredText(canvas, "+", x + PlusW / 2f, boxY + BoxH / 2f, 15f, ColPlus, PlusW);
        return x + PlusW + Gap;
    }

    private static void DrawOptionalBox(ICanvas canvas, float x, float y, float w, float h,
        string label, string sublabel, Color bg, Color stroke, Color fg)
    {
        canvas.FillColor = bg;
        canvas.FillRoundedRectangle(x, y, w, h, 6);

        canvas.StrokeColor = stroke;
        canvas.StrokeSize = 1.5f;
        canvas.StrokeDashPattern = [4f, 3f];
        canvas.DrawRoundedRectangle(x, y, w, h, 6);
        canvas.StrokeDashPattern = null;

        DrawBoxLabels(canvas, x, y, w, h, label, sublabel, fg);
    }

    private static void DrawBox(ICanvas canvas, float x, float y, float w, float h,
        string label, string? sublabel, Color bg, Color stroke, Color fg,
        bool wrapSublabelOnHyphen = false)
    {
        DrawBoxShell(canvas, x, y, w, h, bg, stroke);
        DrawBoxLabels(canvas, x, y, w, h, label, sublabel, fg, wrapSublabelOnHyphen);
    }

    private static void DrawBoxShell(ICanvas canvas, float x, float y, float w, float h, Color bg, Color stroke)
    {
        canvas.FillColor = bg;
        canvas.StrokeColor = stroke;
        canvas.StrokeSize = 1f;
        canvas.FillRoundedRectangle(x, y, w, h, 6);
        canvas.DrawRoundedRectangle(x, y, w, h, 6);
    }

    private static void DrawThaiGlyphInBox(ICanvas canvas, float x, float y, float w, float h, string glyph, Color fg)
    {
        const float inset = 3f;
        canvas.Font = new Microsoft.Maui.Graphics.Font(ThaiFont, ThaiGlyphFontSize);
        canvas.FontSize = ThaiGlyphFontSize;
        canvas.FontColor = fg;
        canvas.DrawString(
            glyph,
            x + inset,
            y + inset,
            w - inset * 2,
            h - inset * 2,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);
    }

    private static void DrawBoxLabels(ICanvas canvas, float x, float y, float w, float h,
        string label, string? sublabel, Color fg, bool wrapSublabelOnHyphen = false)
    {
        float midX = x + w / 2f;
        float textW = w - 6f;

        if (string.IsNullOrEmpty(sublabel))
        {
            DrawCenteredText(canvas, label, midX, y + h / 2f, 14f, fg, textW);
            return;
        }

        if (wrapSublabelOnHyphen && sublabel.Contains('-'))
        {
            var dash = sublabel.IndexOf('-');
            var line1 = sublabel[..(dash + 1)];
            var line2 = sublabel[(dash + 1)..];
            DrawCenteredText(canvas, label, midX, y + h * 0.30f, 13f, fg, textW);
            DrawCenteredText(canvas, line1, midX, y + h * 0.58f, 8f, fg.WithAlpha(0.85f), textW);
            DrawCenteredText(canvas, line2, midX, y + h * 0.78f, 8f, fg.WithAlpha(0.85f), textW);
        }
        else
        {
            DrawCenteredText(canvas, label, midX, y + h * 0.36f, 13f, fg, textW);
            DrawCenteredText(canvas, sublabel, midX, y + h * 0.72f, 9f, fg.WithAlpha(0.85f), textW);
        }
    }

    private static void DrawCouplingBracket(ICanvas canvas, float x1, float x2, float y)
    {
        canvas.StrokeColor = ColBracket;
        canvas.StrokeSize = 1f;
        float mid = (x1 + x2) / 2f;
        canvas.DrawLine(x1, y, x1, y + BracketDepth);
        canvas.DrawLine(x1, y + BracketDepth, mid, y + BracketDepth);
        canvas.DrawLine(mid, y + BracketDepth, x2, y + BracketDepth);
        canvas.DrawLine(x2, y + BracketDepth, x2, y);
    }

    private static void DrawCenteredText(
        ICanvas canvas, string text, float cx, float cy, float fontSize, Color color, float maxWidth)
    {
        canvas.FontColor = color;
        canvas.FontSize = fontSize;
        float h = fontSize * 1.45f;
        float w = Math.Min(maxWidth, Math.Max(fontSize * Math.Max(text.Length, 1) * 0.62f, 16f));
        canvas.DrawString(text, cx - w / 2f, cy - h / 2f, w, h, HorizontalAlignment.Center, VerticalAlignment.Center);
    }
}
