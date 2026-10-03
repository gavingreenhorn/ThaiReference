using ThaiReference.ViewModels;

namespace ThaiReference.Views;

public partial class NumbersView : ContentPage
{
    public NumbersView(NumbersViewModel nvm)
    {
        try
        {
            InitializeComponent();
            BindingContext = nvm;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}