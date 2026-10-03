namespace ThaiReference.Models.ToneRules;

/// <summary>One selectable cell (or header) in the tone-rules chart.</summary>
public sealed class ToneRuleCell
{
	public required string Id { get; init; }
	public required string TitleId { get; init; }
	public required string DetailId { get; init; }
	/// <summary>Chart glyph (M/L/F/H/R), tone mark, or empty for header-only cells.</summary>
	public string Glyph { get; init; } = "";
	/// <summary>Tone chart letter M/L/F/H/R for localized tone name; empty if not a tone result cell.</summary>
	public string ToneKey { get; init; } = "";
}
