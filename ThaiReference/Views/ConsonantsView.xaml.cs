using ThaiReference.ViewModels;

namespace ThaiReference.Views;

public partial class ConsonantsView : ContentPage
{
	public ConsonantsView(ConsonantsViewModel cvm)
	{
		try
		{
            InitializeComponent();
            BindingContext = cvm;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}