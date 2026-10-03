using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.Audio;
using ThaiReference.Messages;
using ThaiReference.Models;
using ThaiReference.Models.Interfaces;
using ThaiReference.Contracts;
using ThaiReference.Utilities.Extensions;
#if ANDROID
using Android.Util;
#endif

namespace ThaiReference.ViewModels
{
    public partial class DetailViewModel : ObservableObject, IQueryAttributable, IVoiced, IRomanizationReceiver
    {
        public RelayCommand<object> PronounceCommand { get; init; }
        public IAudioManager AudioManager { get; init; }

        [ObservableProperty]
        IHaveDescription? descriptionItem;

        [ObservableProperty]
        string romanizationOption = "IPA";

        [ObservableProperty]
        bool isSwiping = false;

        public DetailViewModel(IAudioManager audioManager)
        {
            AudioManager = audioManager;
            PronounceCommand = new RelayCommand<object>(async (item) => await this.Pronounce(item));
            WeakReferenceMessenger.Default.Register<DetailViewModel, RomanizationOptionChanged>(this, (r, m) => RomanizationOption = m.Value);
        }

        [RelayCommand]
        public void Appearing()
        {
            try
            {
#if ANDROID
                Log.Debug(GetType().Name, "Appeared");
#endif
            }
            catch (Exception ex)
            {
#if ANDROID
                Log.Error(GetType().Name, $"Exception thrown on appearing: {ex.Message}");
#endif
            }
        }

        [RelayCommand]
        public void Disappearing()
        {
            try
            {
#if ANDROID
                Log.Debug(GetType().Name, "Disappeared");
#endif
            }
            catch (Exception ex)
            {
#if ANDROID
                Log.Error(GetType().Name, $"Exception thrown on disappearing: {ex.Message}");
#endif
            }
        }

        public void Receive(RomanizationOptionChanged message)
        {
            RomanizationOption = message.Value;
        }

        public void OnSwiping(object sender, SwipeChangingEventArgs e)
        {
            IsSwiping = !IsSwiping;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query != null && query.TryGetValue("Item", out object? item))
            {
                if (item != null && item is IHaveDescription descItem)
                {
                    switch (descItem)
                    {
                        case IConsonant consonant:
                            DescriptionItem = consonant;
                            break;
                        case IVowel vowel:
                            DescriptionItem = vowel;
                            break;
                        case IMark mark:
                            DescriptionItem = mark;
                            break;
                        case IPronoun pronoun:
                            DescriptionItem = pronoun;
                            break;
                        case GridPositionCell cell:
                            DescriptionItem = cell;
                            break;
                        case Weekday day:
                            DescriptionItem = day;
                            break;
                    }
                }
            }         
        }
    }
}
