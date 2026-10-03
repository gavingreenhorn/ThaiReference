using ThaiReference.ViewModels;

namespace ThaiReference.Views;

public partial class GraphemesBySound : ContentView
{
	public GraphemesBySound(GraphemesBySoundViewModel gbsvm)
	{
        try
        {
            InitializeComponent();
            BindingContext = gbsvm;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}