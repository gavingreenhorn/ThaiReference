using ThaiReference.ViewModels;

namespace ThaiReference.Views;

public partial class SettingsView : ContentPage
{
	public SettingsView(SettingsViewModel svm)
	{
		InitializeComponent();
		BindingContext = svm;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext is SettingsViewModel vm)
			vm.AppearingCommand.Execute(null);
	}
}
