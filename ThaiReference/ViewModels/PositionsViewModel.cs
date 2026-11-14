using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.Audio;
using ThaiReference.Contracts;
using ThaiReference.DataAccess;
using ThaiReference.Messages;
using ThaiReference.Models.BaseModels;
using ThaiReference.Models.Interfaces;
using ThaiReference.Utilities;
using ThaiReference.Utilities.Extensions;
using ThaiReference.Views.Drawables;

namespace ThaiReference.ViewModels;

public partial class PositionsViewModel : ObservableObject, IVoiced, IRomanizationReceiver
{
	/// <summary>under, above, left, right — shown as one tappable card.</summary>
	public static readonly int[] AroundPositionIds = [10, 3, 13, 14];

	public RelayCommand<object> PronounceCommand { get; init; }
	public IAudioManager AudioManager { get; init; }
	public IReadOnlyList<PositionWord> Positions { get; }

	[ObservableProperty] private string romanizationOption = "IPA";
	[ObservableProperty] private PositionWord? currentItem;

	public PositionsViewModel(IDataServiceFactory dsf, IAudioManager audioManager)
	{
		IDataService ds = dsf.Get("thai");
		AudioManager = audioManager;
		WeakReferenceMessenger.Default.Register(this);
		PronounceCommand = new RelayCommand<object>(async item => await this.Pronounce(item));

		var byId = ds.Positions.ToDictionary(p => p.Id);
		var around = AroundPositionIds
			.Where(byId.ContainsKey)
			.Select(id => byId[id])
			.ToList();
		var rest = ds.Positions
			.Where(p => !AroundPositionIds.Contains(p.Id))
			.OrderBy(p => p.Id)
			.Select(p => new PositionWord(p, BoxSceneDrawable.ForPositionId(p.Id)));

		var cards = new List<PositionWord>();
		if (around.Count > 0)
			cards.Add(new PositionAroundWord(around));
		cards.AddRange(rest);
		Positions = cards;
		CurrentItem = Positions.ElementAtOrDefault(0);
	}

	public void Receive(RomanizationOptionChanged message) => RomanizationOption = message.Value;

	public void ActivateItem(PositionWord item)
	{
		if (ReferenceEquals(CurrentItem, item))
			return;
		CurrentItem = item;
	}
}

public partial class PositionWord : ObservableObject
{
	public IPosition Source { get; protected set; }

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(Header))]
	private string thai = string.Empty;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(Header))]
	private string english = string.Empty;

	public string Header => string.IsNullOrWhiteSpace(English) ? Thai : $"{Thai} - {English}";
	[ObservableProperty] private string exampleThai = string.Empty;
	[ObservableProperty] private ITranscription exampleTranscription = new Transcription();
	[ObservableProperty] private IWord pronounceTarget = new Word("", new Transcription(), []);

	public IDrawable Scene { get; protected set; } = null!;

	public PositionWord(IPosition position, IDrawable scene)
	{
		Source = position;
		Scene = scene;
		Apply(position);
	}

	protected PositionWord()
	{
		Source = null!;
	}

	protected void Apply(IPosition position)
	{
		Source = position;
		Thai = position.Word.Value;
		English = position.Lexeme;
		ExampleThai = position.Example.Spelling;
		ExampleTranscription = MergeTranscription(position.Example.Words);
		PronounceTarget = new Word(ExampleThai, ExampleTranscription, [position.Lexeme]);
	}

	public virtual bool HandleSceneTap(float x, float y) => false;

	protected static ITranscription MergeTranscription(IReadOnlyList<IWord> words)
	{
		static string Join(IReadOnlyList<IWord> parts, Func<ITranscription, string?> pick) =>
			string.Join(" ", parts
				.Select(w => pick(w.Transcription))
				.Where(s => !string.IsNullOrWhiteSpace(s)));

		return new Transcription(
			Join(words, t => t.Paiboon),
			Join(words, t => t.IPA));
	}
}

/// <summary>Single carousel card: all four balls; tap one to select (halo like days/planets).</summary>
public sealed class PositionAroundWord : PositionWord
{
	readonly Dictionary<AroundSlot, IPosition> _bySlot;
	readonly AroundBoxDrawable _drawable;

	public PositionAroundWord(IReadOnlyList<IPosition> variants)
	{
		if (variants.Count == 0)
			throw new ArgumentException("Need at least one position.", nameof(variants));

		_bySlot = variants.ToDictionary(p => AroundBoxDrawable.SlotForPositionId(p.Id));
		var initial = _bySlot.ContainsKey(AroundSlot.Above)
			? AroundSlot.Above
			: _bySlot.Keys.First();
		_drawable = new AroundBoxDrawable { Slot = initial };
		Scene = _drawable;
		ApplySlot(initial);
	}

	public override bool HandleSceneTap(float x, float y)
	{
		var hit = _drawable.HitTestBall(x, y);
		if (hit is null || hit == _drawable.Slot)
			return false;

		ApplySlot(hit.Value);
		return true;
	}

	void ApplySlot(AroundSlot slot)
	{
		if (!_bySlot.TryGetValue(slot, out var position))
			return;
		_drawable.Slot = slot;
		Apply(position);
	}
}
