using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.Audio;

namespace ThaiReference.Contracts;

public interface IVoiced
{
    IAudioManager AudioManager { get; init; }
    RelayCommand<object> PronounceCommand { get; init; }
}
