using ThaiReference.ViewModels;

namespace ThaiReference.Views;

public partial class MinimalSetsView : ContentPage
{
    public MinimalSetsView(MinimalSetsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
