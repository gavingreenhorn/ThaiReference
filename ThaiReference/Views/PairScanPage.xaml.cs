using BarcodeScanning;
using ThaiReference.Account;
using ThaiReference.DataAccess;
using ThaiReference.ViewModels;

namespace ThaiReference.Views;

public partial class PairScanPage : ContentPage
{
    readonly DeviceIdStore _deviceIdStore;
    readonly AccountClient _accountClient;
    readonly SettingsViewModel _settings;
    bool _handling;

    public PairScanPage(DeviceIdStore deviceIdStore, AccountClient accountClient, SettingsViewModel settings)
    {
        InitializeComponent();
        _deviceIdStore = deviceIdStore;
        _accountClient = accountClient;
        _settings = settings;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Methods.AskForRequiredPermissionAsync();
        BarcodeScanner.CameraEnabled = true;
    }

    protected override void OnDisappearing()
    {
        BarcodeScanner.CameraEnabled = false;
        base.OnDisappearing();
    }

    async void OnDetectionFinished(object? sender, OnDetectionFinishedEventArg e)
    {
        if (_handling || e.BarcodeResults.Count == 0)
            return;
        var token = e.BarcodeResults.Select(r => r.RawValue).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
        if (string.IsNullOrWhiteSpace(token))
            return;

        _handling = true;
        BarcodeScanner.CameraEnabled = false;
        try
        {
            var uuid = await _deviceIdStore.GetOrCreateAsync();
            var info = await _accountClient.CompletePairAsync(
                uuid,
                token,
                DeviceInfo.Current.Platform.ToString(),
                AppInfo.Current.BuildString);
            if (info is not null)
                await _settings.ApplyPairedAsync(info);
            await Shell.Current.GoToAsync("..");
        }
        catch
        {
            _handling = false;
            BarcodeScanner.CameraEnabled = true;
        }
    }
}
