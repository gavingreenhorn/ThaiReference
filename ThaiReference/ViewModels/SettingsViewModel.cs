using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using QRCoder;
using ThaiReference.Account;
using ThaiReference.DataAccess;
using ThaiReference.Messages;
using ThaiReference.Ui;

namespace ThaiReference.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    readonly DeviceIdStore _deviceIdStore;
    readonly AccountClient _accountClient;
    readonly ApiAvailabilityMonitor _availability;

    public SettingsViewModel(
        AppShellViewModel shell,
        DeviceIdStore deviceIdStore,
        AccountClient accountClient,
        ApiAvailabilityMonitor availability)
    {
        Shell = shell;
        _deviceIdStore = deviceIdStore;
        _accountClient = accountClient;
        _availability = availability;
        PreferRecorded = Preferences.Get("PreferRecorded", false);
        WeakReferenceMessenger.Default.Register<UiLanguageChanged>(this, (_, m) => ApplyLanguage(m.Value));
        ApplyLanguage(UiLanguageHelper.Read());
    }

    public AppShellViewModel Shell { get; }

    public IReadOnlyList<UiLanguageOption> UiLanguageOptions => Shell.UiLanguageOptions;

    public UiLanguageOption SelectedUiLanguageOption
    {
        get => Shell.SelectedUiLanguageOption;
        set
        {
            if (ReferenceEquals(Shell.SelectedUiLanguageOption, value) || value is null)
                return;
            Shell.SelectedUiLanguageOption = value;
            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    bool preferRecorded;

    [ObservableProperty]
    string preferRecordingsLabel = "";

    [ObservableProperty]
    string uiLanguageLabel = "";

    [ObservableProperty]
    string devicesLabel = "";

    [ObservableProperty]
    string thisDeviceLabel = "";

    [ObservableProperty]
    string membersLabel = "";

    [ObservableProperty]
    string unlinkLabel = "";

    [ObservableProperty]
    string showQrLabel = "";

    [ObservableProperty]
    string hideQrLabel = "";

    [ObservableProperty]
    string scanQrLabel = "";

    [ObservableProperty]
    string emailLabel = "";

    [ObservableProperty]
    string emailHint = "";

    [ObservableProperty]
    string attachEmailLabel = "";

    [ObservableProperty]
    string confirmEmailLabel = "";

    [ObservableProperty]
    string recoverLabel = "";

    [ObservableProperty]
    string confirmRecoverLabel = "";

    [ObservableProperty]
    string codeHint = "";

    [ObservableProperty]
    string thisDeviceUuid = "";

    [ObservableProperty]
    string emailValue = "";

    [ObservableProperty]
    string emailInput = "";

    [ObservableProperty]
    string recoverInput = "";

    [ObservableProperty]
    string emailCodeInput = "";

    [ObservableProperty]
    string recoverCodeInput = "";

    [ObservableProperty]
    string statusMessage = "";

    [ObservableProperty]
    bool qrVisible;

    [ObservableProperty]
    ImageSource? qrImage;

    public ObservableCollection<DeviceMemberItem> Members { get; } = [];

    partial void OnPreferRecordedChanged(bool value)
    {
        if (Preferences.Get("PreferRecorded", false) != value)
            Preferences.Set("PreferRecorded", value);
    }

    [RelayCommand]
    public async Task Appearing() => await RefreshAsync();

    [RelayCommand]
    async Task Refresh() => await RefreshAsync();

    [RelayCommand]
    async Task ShowQr()
    {
        var pair = await WithDevice((uuid, platform, build) =>
            _accountClient.StartPairAsync(uuid, platform, build));
        if (pair is null || string.IsNullOrWhiteSpace(pair.Token))
        {
            StatusMessage = UiLabels.Get(UiLabels.Account.StatusOffline, CurrentLanguage());
            return;
        }

        QrImage = QrFromToken(pair.Token);
        QrVisible = true;
    }

    [RelayCommand]
    void HideQr()
    {
        QrVisible = false;
        QrImage = null;
    }

    [RelayCommand]
    async Task ScanQr()
    {
        await Microsoft.Maui.Controls.Shell.Current.GoToAsync(nameof(Views.PairScanPage));
    }

    [RelayCommand]
    async Task Unlink(DeviceMemberItem? member)
    {
        if (member is null)
            return;
        var info = await WithDevice((uuid, platform, build) =>
            _accountClient.UnlinkAsync(uuid, member.Uuid, platform, build));
        if (info is null)
        {
            StatusMessage = UiLabels.Get(UiLabels.Account.StatusOffline, CurrentLanguage());
            return;
        }

        ApplyInfo(info);
        StatusMessage = UiLabels.Get(UiLabels.Account.StatusUnlinked, CurrentLanguage());
    }

    [RelayCommand]
    async Task AttachEmail()
    {
        var email = EmailInput.Trim();
        if (string.IsNullOrWhiteSpace(email))
            return;
        var ok = await WithDevice((uuid, platform, build) =>
            _accountClient.RequestEmailAsync(uuid, email, platform, build));
        StatusMessage = ok is { Ok: true }
            ? UiLabels.Get(UiLabels.Account.StatusEmailSent, CurrentLanguage())
            : UiLabels.Get(UiLabels.Account.StatusOffline, CurrentLanguage());
    }

    [RelayCommand]
    async Task ConfirmEmail()
    {
        var code = EmailCodeInput.Trim();
        if (string.IsNullOrWhiteSpace(code))
            return;
        var ok = await WithDevice((uuid, platform, build) =>
            _accountClient.ConfirmEmailAsync(uuid, code, platform, build));
        if (ok is not { Ok: true })
        {
            StatusMessage = UiLabels.Get(UiLabels.Account.StatusOffline, CurrentLanguage());
            return;
        }

        EmailCodeInput = "";
        await RefreshAsync();
        StatusMessage = UiLabels.Get(UiLabels.Account.StatusEmailConfirmed, CurrentLanguage());
    }

    [RelayCommand]
    async Task Recover()
    {
        var email = RecoverInput.Trim();
        if (string.IsNullOrWhiteSpace(email))
            return;
        var ok = await WithDevice((uuid, platform, build) =>
            _accountClient.RequestRecoverAsync(uuid, email, platform, build));
        StatusMessage = ok is { Ok: true }
            ? UiLabels.Get(UiLabels.Account.StatusRecoverSent, CurrentLanguage())
            : UiLabels.Get(UiLabels.Account.StatusOffline, CurrentLanguage());
    }

    [RelayCommand]
    async Task ConfirmRecover()
    {
        var code = RecoverCodeInput.Trim();
        if (string.IsNullOrWhiteSpace(code))
            return;
        var ok = await WithDevice((uuid, platform, build) =>
            _accountClient.ConfirmRecoverAsync(uuid, code, platform, build));
        if (ok is not { Ok: true })
        {
            StatusMessage = UiLabels.Get(UiLabels.Account.StatusOffline, CurrentLanguage());
            return;
        }

        RecoverCodeInput = "";
        await RefreshAsync();
        StatusMessage = UiLabels.Get(UiLabels.Account.StatusRecovered, CurrentLanguage());
    }

    public async Task ApplyPairedAsync(AccountClient.AccountInfoResponse info)
    {
        ApplyInfo(info);
        StatusMessage = UiLabels.Get(UiLabels.Account.StatusPaired, CurrentLanguage());
        await Task.CompletedTask;
    }

    async Task RefreshAsync()
    {
        try
        {
            ThisDeviceUuid = await _deviceIdStore.GetOrCreateAsync();
            if (!_availability.IsUp)
                return;
            var info = await WithDevice((uuid, platform, build) =>
                _accountClient.GetInfoAsync(uuid, platform, build));
            if (info is not null)
                ApplyInfo(info);
        }
        catch
        {
            // offline: keep last shown members
        }
    }

    void ApplyInfo(AccountClient.AccountInfoResponse info)
    {
        ThisDeviceUuid = info.DeviceUuid.ToString("D");
        EmailValue = info.EmailSet ? info.Email ?? "" : "";
        if (info.EmailSet && string.IsNullOrWhiteSpace(EmailInput))
            EmailInput = EmailValue;
        Members.Clear();
        foreach (var member in info.Devices ?? [])
            Members.Add(new DeviceMemberItem(member, UnlinkCommand));
    }

    async Task<T?> WithDevice<T>(Func<string, string, string, Task<T?>> call)
        where T : class
    {
        try
        {
            var uuid = await _deviceIdStore.GetOrCreateAsync();
            return await call(uuid, DeviceInfo.Current.Platform.ToString(), AppInfo.Current.BuildString);
        }
        catch
        {
            return null;
        }
    }

    static ImageSource QrFromToken(string token)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(token, QRCodeGenerator.ECCLevel.M);
        var png = new PngByteQRCode(data).GetGraphic(12);
        return ImageSource.FromStream(() => new MemoryStream(png));
    }

    void ApplyLanguage(UiLanguage language)
    {
        PreferRecordingsLabel = UiLabels.Get(UiLabels.Chrome.PreferRecordings, language);
        UiLanguageLabel = UiLabels.Get(UiLabels.Chrome.UiLanguage, language);
        DevicesLabel = UiLabels.Get(UiLabels.Account.Devices, language);
        ThisDeviceLabel = UiLabels.Get(UiLabels.Account.ThisDevice, language);
        MembersLabel = UiLabels.Get(UiLabels.Account.Members, language);
        UnlinkLabel = UiLabels.Get(UiLabels.Account.Unlink, language);
        ShowQrLabel = UiLabels.Get(UiLabels.Account.ShowQr, language);
        HideQrLabel = UiLabels.Get(UiLabels.Account.HideQr, language);
        ScanQrLabel = UiLabels.Get(UiLabels.Account.ScanQr, language);
        EmailLabel = UiLabels.Get(UiLabels.Account.Email, language);
        EmailHint = UiLabels.Get(UiLabels.Account.EmailHint, language);
        AttachEmailLabel = UiLabels.Get(UiLabels.Account.AttachEmail, language);
        ConfirmEmailLabel = UiLabels.Get(UiLabels.Account.ConfirmEmail, language);
        RecoverLabel = UiLabels.Get(UiLabels.Account.Recover, language);
        ConfirmRecoverLabel = UiLabels.Get(UiLabels.Account.ConfirmRecover, language);
        CodeHint = UiLabels.Get(UiLabels.Account.CodeHint, language);
        OnPropertyChanged(nameof(SelectedUiLanguageOption));
    }

    static UiLanguage CurrentLanguage() => UiLanguageHelper.Read();
}
