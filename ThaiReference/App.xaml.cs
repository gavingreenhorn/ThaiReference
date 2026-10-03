using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using ThaiReference.Account;
using ThaiReference.Converters;
using ThaiReference.DataAccess;
using ThaiReference.ViewModels;

namespace ThaiReference
{
    public partial class App : Application
    {
        private readonly AppShellViewModel asvm;

        public App(
            AppShellViewModel asvm,
            ArabicThaiNumberConverter converter,
            ApiAvailabilityMonitor availability,
            AccountSyncService accountSync)
        {
#if DEBUG
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
#endif
            this.asvm = asvm;
            try
            {
                InitializeComponent();
                // registered here since the converter depends on DI
                Resources["ArabicToThai"] = converter;
                var wasOffline = false;
                availability.Changed += up => MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (up)
                    {
                        if (!wasOffline)
                            return;
                        wasOffline = false;
                        await Toast.Make("Back online.", ToastDuration.Long, 14).Show();
                    }
                    else
                    {
                        wasOffline = true;
                        await Toast.Make("Showing offline data — server unavailable.", ToastDuration.Long, 14).Show();
                    }
                });
                availability.Changed += up =>
                {
                    if (up)
                        _ = accountSync.TrySyncAsync();
                };
                _ = CheckApiAndAccountAsync(availability, accountSync);
                availability.Start(TimeSpan.FromSeconds(10));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error initializing the App: {ex.Message}\n{ex.StackTrace}");
            }
        }

#if DEBUG
        private void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[Exception] {e.Exception.Message}\n{e.Exception.StackTrace}");
        }
#endif

        static async Task CheckApiAndAccountAsync(ApiAvailabilityMonitor availability, AccountSyncService accountSync)
        {
            await availability.CheckOnceAsync();
            if (availability.IsUp)
                await accountSync.TrySyncAsync();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell(asvm));
        }
    }
}
