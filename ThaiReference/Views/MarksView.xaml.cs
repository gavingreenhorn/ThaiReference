using ThaiReference.ViewModels;

namespace ThaiReference.Views;

public partial class MarksView : ContentPage
{
	public MarksView(MarksViewModel mvm)
	{
		InitializeComponent();
		BindingContext = mvm;
	}
}