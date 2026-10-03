using ThaiReference.ViewModels;

namespace ThaiReference.Views;

public partial class DaysView : ContentPage
{
    public DaysView(DaysViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
