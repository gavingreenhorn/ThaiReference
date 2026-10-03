using ThaiReference.ViewModels;

namespace ThaiReference.Views;

public partial class HomophonesView : ContentPage
{
    public HomophonesView(HomophonesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
