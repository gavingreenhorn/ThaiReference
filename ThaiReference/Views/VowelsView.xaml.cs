using ThaiReference.ViewModels;

namespace ThaiReference.Views;

public partial class VowelsView : ContentPage
{
	public VowelsView(VowelsViewModel vvm)
	{
		InitializeComponent();
		BindingContext = vvm;
	}
}