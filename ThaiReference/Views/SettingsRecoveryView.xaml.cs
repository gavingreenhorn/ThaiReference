using ThaiReference.ViewModels;

namespace ThaiReference.Views;

public partial class SettingsRecoveryView : ContentPage
{
    public SettingsRecoveryView(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SettingsViewModel viewModel)
            viewModel.AppearingCommand.Execute(null);
    }
}
