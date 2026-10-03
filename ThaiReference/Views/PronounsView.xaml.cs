using ThaiReference.ViewModels;

namespace ThaiReference.Views;

public partial class PronounsView : ContentPage
{
    public PronounsView(PronounsViewModel pvm)
    {
        InitializeComponent();
        BindingContext = pvm;
    }
}