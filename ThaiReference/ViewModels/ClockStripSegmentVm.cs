namespace ThaiReference.ViewModels;

/// <summary>One time-of-day band on the clock wheel (built entirely in <see cref="ThaiClockViewModel"/>).</summary>
public sealed record ClockStripSegmentVm(
    int Ordinal,
    string Thai,
    int StartHour,
    int EndHour,
    string ColorHex);
