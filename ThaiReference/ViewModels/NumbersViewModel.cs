using ThaiReference.DataAccess;
using ThaiReference.Models.Interfaces;
using ThaiReference.Utilities;
using ThaiReference.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThaiReference.Contracts;
using Plugin.Maui.Audio;
using ThaiReference.Utilities.Extensions;
using CommunityToolkit.Mvvm.Messaging;
using ThaiReference.Messages;


namespace ThaiReference.ViewModels
{
    public partial class NumbersViewModel : ObservableObject, IVoiced, IRecipient<RomanizationOptionChanged>
    {
        private const string defaultRomanizationOption = "IPA";
        public RelayCommand<object> PronounceCommand { get; init; }
        public IAudioManager AudioManager { get; init; }
        private readonly INumeral[] numerals_array;

        public NumbersViewModel(IDataServiceFactory dsf, IAudioManager audioManager, INumeral[] numerals)
        {
            IDataService ds = dsf.Get("thai");
            AudioManager = audioManager;
            PronounceCommand = new RelayCommand<object>(async (item) => await this.Pronounce(item));
            numerals_array = numerals;
            Numbers = new ObservableCollection<ThaiNumber>(GenerateThaiNumbers(Enumerable.Range(0, 100)));
            Units = new ObservableCollection<ThaiNumber>(GenerateThaiNumbers([10, 100, 1000, 10000, 100000, 1000000]));
        }

        [ObservableProperty]
        string romanizationOption = defaultRomanizationOption;

        [ObservableProperty]
        ObservableCollection<ThaiNumber> units;

        [ObservableProperty]
        ObservableCollection<ThaiNumber> numbers;

        public void Receive(RomanizationOptionChanged message)
        {
            RomanizationOption = message.Value;
        }

        private ThaiNumber GetThaiNumber(int ar_number)
        {
            return new ThaiNumber(ar_number, numerals_array);
        }

        private IEnumerable<ThaiNumber> GenerateThaiNumbers(IEnumerable<int> arabicNumbers)
        {
            foreach (int ar_number in arabicNumbers)
            {
                yield return GetThaiNumber(ar_number);
            }
        }
    }
}
