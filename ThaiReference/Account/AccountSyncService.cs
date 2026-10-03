using ThaiReference.DataAccess;

namespace ThaiReference.Account;

public sealed class AccountSyncService(
    DeviceIdStore deviceIdStore,
    AccountClient accountClient,
    ApiAvailabilityMonitor availability)
{
    public async Task<AccountClient.AccountInfoResponse?> TrySyncAsync(CancellationToken cancellationToken = default)
    {
        if (!availability.IsUp)
            return null;
        try
        {
            var uuid = await deviceIdStore.GetOrCreateAsync().ConfigureAwait(false);
            return await accountClient.CheckinAsync(
                    uuid,
                    DeviceInfo.Current.Platform.ToString(),
                    AppInfo.Current.BuildString,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
    }
}
