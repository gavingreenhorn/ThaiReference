using ThaiReference.ViewModels;

namespace ThaiReference.Views;

[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class DetailView : ContentPage
{
    public DetailView(DetailViewModel vm)
	{
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }
}