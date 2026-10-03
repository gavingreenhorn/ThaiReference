using ThaiReference.Contracts;
using ThaiReference.Models.BaseModels;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models;

public class GridPositionCell : IGridPositionCell, IHaveDescription
{
    public int Ordinal { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
    public string? TileStyle { get; set; }
    public string? Label { get; set; }
    public List<Word> Words { get; set; } = [];

    IReadOnlyList<IWord> IGridPositionCell.Words => Words;

    public string ItemNumber => $"ช่อง {Ordinal}";
    public string WordsString => string.Join(" ", Words.Select(w => w.Value));
    public string Caption => $"{WordsString}";
    public string Audible => $"{WordsString}\n{ItemNumber}";
}
