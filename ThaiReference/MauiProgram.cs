using BarcodeScanning;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
using ThaiReference.Account;
using System.Globalization;
using System.Reflection;
using ThaiReference.Converters;
using ThaiReference.DataAccess;
using ThaiReference.Utilities;
using ThaiReference.Utilities.Extensions;
using ThaiReference.ViewModels;
using ThaiReference.Views;

namespace ThaiReference
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseBarcodeScanning()
                .AddAudio()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSans");
                    fonts.AddFont("NotoSerifThai-Regular.ttf", "NotoSerifThai");
                    fonts.AddFont("NotoSansThai-Regular.ttf", "NotoSansThai");
                    // Alias + filename-without-extension (Android often resolves the latter).
                    fonts.AddFont("MaterialIconsSharp-Regular.otf", "MaterialIconsSharp");
                    fonts.AddFont("MaterialIconsSharp-Regular.otf", "MaterialIconsSharp-Regular");
                    fonts.AddFont("DSEG7Classic-Regular.ttf", "DSEG7Classic");
                    fonts.AddFont("DSEG7Classic-Regular.ttf", "DSEG7Classic-Regular");
                })
                .ConfigureMauiHandlers(handlers =>
                {
#if ANDROID
                    handlers.AddHandler<Shell, CustomShellHandler>();
#endif
                });
            builder.Logging.AddConsole();

            using var configStream = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream("ThaiReference.appsettings.json")
                ?? throw new InvalidOperationException("appsettings.json not found");
            builder.Configuration.AddJsonStream(configStream);
            VoicedExtensions.Http.BaseAddress ??= new Uri(ApiDataService.GetBaseUrl(builder.Configuration));
            builder.Services.AddThaiDataServices();
            builder.Services.AddSingleton<AccountClient>();
            builder.Services.AddSingleton<DeviceIdStore>();
            builder.Services.AddSingleton<AccountSyncService>();
            builder.Services.AddSingleton<IDataServiceFactory, DataServiceFactory>();
            builder.Services.AddTransient<PairScanPage>();

            // global dependencies
            builder.Services.AddSingleton(sp =>
                sp.GetRequiredService<IDataServiceFactory>().Get("thai").Numerals.ToArray()
            );
            builder.Services.AddSingleton<ArabicThaiNumberConverter>();
            builder.Services.AddTransient<ThaiClockView>();
            builder.Services.AddTransient<ThaiClockViewModel>();

            // viewmodels / Singleton
            builder.Services.AddSingleton<AppShellViewModel>();
            builder.Services.AddSingleton<ConsonantsViewModel>();
            builder.Services.AddSingleton<VowelsViewModel>();
            builder.Services.AddSingleton<MarksViewModel>();
            builder.Services.AddSingleton<ConsonantClustersViewModel>();
            builder.Services.AddSingleton<HomophonesViewModel>();
            builder.Services.AddSingleton<MinimalSetsViewModel>();
            builder.Services.AddSingleton<NumbersViewModel>();
            builder.Services.AddSingleton<PronounsViewModel>();
            builder.Services.AddSingleton<PositionsViewModel>();
            builder.Services.AddSingleton<GridPositionsViewModel>();
            builder.Services.AddSingleton<DirectionsViewModel>();
            builder.Services.AddSingleton<ToneRulesViewModel>();
            builder.Services.AddSingleton<DaysViewModel>();
            builder.Services.AddSingleton<MonthsViewModel>();
            builder.Services.AddSingleton<ColorsViewModel>();
            builder.Services.AddSingleton<SettingsViewModel>();
            // viewmodels / Transient
            builder.Services.AddTransient<AllConsonantsViewModel>();
            builder.Services.AddTransient<ConsonantsBySoundViewModel>();
            builder.Services.AddTransient<ConsonantsBySoundTypeViewModel>();
            builder.Services.AddTransient<AllVowelsViewModel>();
            builder.Services.AddTransient<GraphemesBySoundViewModel>();
            builder.Services.AddTransient<DetailViewModel>();
            // views / Singleton
            builder.Services.AddSingleton<AppShell>();

            builder.Services.AddSingleton<ConsonantsView>();
            builder.Services.AddSingleton<VowelsView>();
            builder.Services.AddSingleton<MarksView>();
            builder.Services.AddSingleton<ConsonantClustersView>();
            builder.Services.AddSingleton<HomophonesView>();
            builder.Services.AddSingleton<MinimalSetsView>();
            builder.Services.AddSingleton<NumbersView>();
            builder.Services.AddSingleton<PronounsView>();
            builder.Services.AddSingleton<DirectionsView>();
            builder.Services.AddSingleton<PositionsView>();
            builder.Services.AddSingleton<GridPositionsView>();
            builder.Services.AddSingleton<ToneRulesView>();
            builder.Services.AddSingleton<DaysView>();
            builder.Services.AddSingleton<MonthsView>();
            builder.Services.AddSingleton<ColorsView>();
            builder.Services.AddSingleton<SettingsView>();
            // views / Transient
            builder.Services.AddTransient<AllVowels>();
            builder.Services.AddTransient<GraphemesBySound>();
            builder.Services.AddTransient<DetailView>();
#if DEBUG
            builder.Logging.AddDebug().SetMinimumLevel(LogLevel.Debug);
#endif
            return builder.Build();
        }
    }
}
