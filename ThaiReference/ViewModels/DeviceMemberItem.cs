using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThaiReference.DataAccess;
using ThaiReference.Ui;

namespace ThaiReference.ViewModels;

public partial class DeviceMemberItem : ObservableObject
{
    public DeviceMemberItem(AccountClient.DeviceMemberDto dto, IRelayCommand<DeviceMemberItem> unlinkCommand,
        IRelayCommand<DeviceMemberItem> transferOwnerCommand, bool isOwner)
    {
        Uuid = dto.Uuid;
        Platform = dto.Platform ?? "";
        Model = dto.Model ?? "";
        Build = dto.Build ?? "";
        ThisDevice = dto.ThisDevice;
        Owner = dto.Owner;
        UnlinkCommand = unlinkCommand;
        TransferOwnerCommand = transferOwnerCommand;
        CanReceiveOwnership = isOwner && !dto.Owner;
    }

    public Guid Uuid { get; }
    public string Platform { get; }
    public string Model { get; }
    public string Build { get; }
    public bool ThisDevice { get; }
    public bool Owner { get; }
    public IRelayCommand<DeviceMemberItem> UnlinkCommand { get; }
    public IRelayCommand<DeviceMemberItem> TransferOwnerCommand { get; }
    public bool CanReceiveOwnership { get; }

    public string Display =>
        $"{(string.IsNullOrWhiteSpace(Model) ? (string.IsNullOrWhiteSpace(Platform) ? UiLabels.Get(UiLabels.Account.UnknownDevice, UiLanguageHelper.Read()) : Platform) : Model)} · {ShortUuid}";

    public string ShortUuid => $"{Uuid.ToString("D")[..4]}...{Uuid.ToString("D")[^4..]}";
}
