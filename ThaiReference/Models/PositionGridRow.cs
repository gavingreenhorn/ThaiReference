using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models;

public sealed record PositionGridRow(IReadOnlyList<IGridPositionCell> Cells);
