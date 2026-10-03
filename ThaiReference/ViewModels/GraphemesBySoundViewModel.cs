using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ThaiReference.DataAccess;
using ThaiReference.Contracts;
using ThaiReference.Messages;
using ThaiReference.Models.Interfaces;
using ThaiReference.Utilities;
using ThaiReference.Utilities.Extensions;
using System.Collections.ObjectModel;
using ThaiReference.Models;

namespace ThaiReference.ViewModels;

public partial class GraphemesBySoundViewModel : ObservableObject
{
    public GraphemesBySoundViewModel(IDataServiceFactory dsf)
    {
        IDataService ds = dsf.Get("thai");
        VowelItems = [.. ds.Vowels.Select(x => new VowelItem(x))];
    }

    [ObservableProperty]
    string romanizationOption = "IPA";

    [ObservableProperty]
    ObservableCollection<VowelItem> vowelItems = [];
}
