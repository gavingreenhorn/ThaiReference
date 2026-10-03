using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.Maui;

namespace ThaiReference
{
	[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
	public class MainActivity : MauiAppCompatActivity
	{
		protected override void OnCreate(Bundle? savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			// Match Shell flyout / ControlBG3 — avoid Material colorPrimary yellow on system bars.
			if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
			{
				var teal = Android.Graphics.Color.ParseColor("#073B47");
				Window?.SetStatusBarColor(teal);
				Window?.SetNavigationBarColor(teal);
			}
		}
	}
}
