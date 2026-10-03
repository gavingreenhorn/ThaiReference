using ThaiReference.ViewModels;

namespace ThaiReference.Views;

public partial class AllVowels : ContentView
{
    public AllVowels(AllVowelsViewModel avvm)
    {
        try
        {
            InitializeComponent();
            BindingContext = avvm;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}