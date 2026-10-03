using ThaiReference.Ui;
using ThaiReference.ViewModels;
using ThaiReference.Views;

namespace ThaiReference
{
	public partial class AppShell : Shell
	{
		readonly AppShellViewModel _vm;

		public AppShell(AppShellViewModel asvm)
		{
			_vm = asvm;
			InitializeComponent();
			BindingContext = asvm;
			Routing.RegisterRoute("Details", typeof(DetailView));
			Routing.RegisterRoute(nameof(PairScanPage), typeof(PairScanPage));
			asvm.ApplyShellChrome = ApplyChromeLabels;
			ApplyChromeLabels();
		}

		void ApplyChromeLabels()
		{
			var lang = _vm.UiLanguage;
			foreach (var item in Items)
			{
				if (item is FlyoutItem flyout)
					ApplyFlyout(flyout, lang);
			}

			SettingsFooterLabel.Text = UiLabels.Get(UiLabels.Shell.Settings, lang);
		}

		static void ApplyFlyout(FlyoutItem flyout, UiLanguage lang)
		{
			if (!string.IsNullOrEmpty(flyout.Route))
				flyout.Title = UiLabels.ForRoute(flyout.Route, lang);

			foreach (var section in flyout.Items)
			{
				foreach (var content in section.Items)
				{
					if (!string.IsNullOrEmpty(content.Route))
						content.Title = UiLabels.ForRoute(content.Route, lang);
				}
			}
		}

		async void OnSettingsClicked(object? sender, EventArgs e)
		{
			await GoToAsync("//settings");
			FlyoutIsPresented = false;
		}
	}
}
