using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.Audio;
using ThaiReference.Contracts;
using ThaiReference.DataAccess;
using ThaiReference.Messages;
using ThaiReference.Models;
using ThaiReference.Models.BaseModels;
using ThaiReference.Models.Interfaces;
using ThaiReference.Utilities;
using ThaiReference.Utilities.Extensions;

namespace ThaiReference.ViewModels;

public partial class GridPositionsViewModel : ObservableObject, IVoiced, IRomanizationReceiver
{
	public RelayCommand<object> PronounceCommand { get; init; }
	public IAudioManager AudioManager { get; init; }

	public IReadOnlyList<PositionGridRow> GridRows { get; }

	[ObservableProperty] private string romanizationOption = "IPA";
	[ObservableProperty] private IGridPositionCell? selectedCell;
	[ObservableProperty] private ITranscription? selectedTranscription;

	public GridPositionsViewModel(IDataServiceFactory dsf, IAudioManager audioManager)
	{
		IDataService ds = dsf.Get("thai");
		AudioManager = audioManager;
		WeakReferenceMessenger.Default.Register(this);
		PronounceCommand = new RelayCommand<object>(async item => await this.Pronounce(item));
		GridRows = ds.GridPositionCells
			.OrderBy(c => c.Row)
			.ThenBy(c => c.Col)
			.GroupBy(c => c.Row)
			.Select(g => new PositionGridRow(g.ToList()))
			.ToList();
	}

	public void Receive(RomanizationOptionChanged message) => RomanizationOption = message.Value;

	[RelayCommand]
	private void SelectCell(IGridPositionCell? cell) => SelectedCell = cell;

	partial void OnSelectedCellChanged(IGridPositionCell? value) =>
		SelectedTranscription = value is null ? null : JoinTranscription(value.Words);

	static Transcription JoinTranscription(IReadOnlyList<IWord> words)
	{
		static string Join(IEnumerable<IWord> src, Func<ITranscription, string?> field) =>
			string.Join(" ", src
				.Select(w => w.Transcription)
				.Where(t => t is not null)
				.Select(t => field(t!))
				.Where(s => !string.IsNullOrWhiteSpace(s)));

		return new Transcription(
			Join(words, t => t.Paiboon),
			Join(words, t => t.IPA));
	}
}
