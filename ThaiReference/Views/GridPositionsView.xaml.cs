using ThaiReference.ViewModels;

namespace ThaiReference.Views;

public partial class GridPositionsView : ContentPage
{
	public GridPositionsView(GridPositionsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
