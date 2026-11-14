using ThaiReference.ViewModels;

namespace ThaiReference.Views;

public partial class RecoverGroupPage : ContentPage
{
    public RecoverGroupPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
