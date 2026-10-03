using CommunityToolkit.Mvvm.Input;

namespace ThaiReference.Contracts;

public interface IDetailsNavigable
{
    public RelayCommand<IHaveDescription> ToDetailsCommand { get; init; }
}
