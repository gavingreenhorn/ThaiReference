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

public partial class ColorsViewModel : ObservableObject, IVoiced, IRomanizationReceiver
{
    public RelayCommand<object> PronounceCommand { get; init; }
    public IAudioManager AudioManager { get; init; }

    public IReadOnlyList<IHueFamily> HueFamilies { get; }
    public IReadOnlyList<IColor> AllColors { get; }

    [ObservableProperty] private string romanizationOption = "IPA";
    [ObservableProperty] private IHueFamily? selectedFamily;
    [ObservableProperty] private IColor? selectedColor;

    public ColorsViewModel(IDataServiceFactory dsf, IAudioManager audioManager)
    {
        IDataService ds = dsf.Get("thai");
        AudioManager = audioManager;
        WeakReferenceMessenger.Default.Register(this);
        PronounceCommand = new RelayCommand<object>(async item => await this.Pronounce(item));

        HueFamilies = ds.HueFamilies.OrderBy(h => h.DisplayOrder).ToList();
        AllColors = HueFamilies.SelectMany(f => f.Colors).ToList();
    }

    public void Receive(RomanizationOptionChanged message) => RomanizationOption = message.Value;

    [RelayCommand]
    private void SelectColor(IColor? color)
    {
        SelectedColor = color;
        if (color != null)
            SelectedFamily = HueFamilies.FirstOrDefault(f => f.Colors.Contains(color));
    }

    [RelayCommand]
    private void SelectFamily(IHueFamily? family)
    {
        SelectedFamily = family;
        SelectedColor = family?.Colors.FirstOrDefault();
    }
}
