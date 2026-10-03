using ThaiReference.Contracts;

namespace ThaiReference.Utilities.Extensions;

public static class DetailsNavigationExtensions
{
    public static async Task ToDetailsAsync(this IDetailsNavigable _, IHaveDescription describedItem)
    {
        ArgumentNullException.ThrowIfNull(describedItem);
        await Shell.Current.GoToAsync($"Details", new ShellNavigationQueryParameters() { { "Item", describedItem } });
    }
}
