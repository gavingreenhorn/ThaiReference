using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.Audio;
using ThaiReference.Contracts;
using ThaiReference.DataAccess;
using ThaiReference.Messages;
using ThaiReference.Models.Interfaces;
using ThaiReference.Utilities;
using ThaiReference.Utilities.Extensions;

namespace ThaiReference.ViewModels;

public partial class DirectionsViewModel : ObservableObject, IVoiced, IRomanizationReceiver
{
	public RelayCommand<object> PronounceCommand { get; init; }
	public IAudioManager AudioManager { get; init; }
	public IReadOnlyList<IDirection> Directions { get; }

	[ObservableProperty] private string romanizationOption = "IPA";
	[ObservableProperty] private IDirection? selectedDirection;

	public DirectionsViewModel(IDataServiceFactory dsf, IAudioManager audioManager)
	{
		IDataService ds = dsf.Get("thai");
		AudioManager = audioManager;
		WeakReferenceMessenger.Default.Register(this);
		PronounceCommand = new RelayCommand<object>(async item => await this.Pronounce(item));
		Directions = ds.Directions.OrderBy(d => d.Id).ToList();
	}

	public void Receive(RomanizationOptionChanged message) => RomanizationOption = message.Value;

	[RelayCommand]
	private void SelectDirection(int degrees) =>
		SelectedDirection = Directions.FirstOrDefault(d => d.Degrees == degrees);
}
