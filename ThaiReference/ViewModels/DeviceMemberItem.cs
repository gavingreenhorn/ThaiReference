using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThaiReference.DataAccess;

namespace ThaiReference.ViewModels;

public partial class DeviceMemberItem : ObservableObject
{
    public DeviceMemberItem(AccountClient.DeviceMemberDto dto, IRelayCommand<DeviceMemberItem> unlinkCommand)
    {
        Uuid = dto.Uuid;
        Platform = dto.Platform ?? "";
        Build = dto.Build ?? "";
        ThisDevice = dto.ThisDevice;
        UnlinkCommand = unlinkCommand;
    }

    public Guid Uuid { get; }
    public string Platform { get; }
    public string Build { get; }
    public bool ThisDevice { get; }
    public IRelayCommand<DeviceMemberItem> UnlinkCommand { get; }

    public string Display =>
        string.IsNullOrWhiteSpace(Platform) ? ShortUuid : $"{Platform} · {ShortUuid}";

    public string ShortUuid => Uuid.ToString("D")[..8];
}
