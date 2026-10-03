using ThaiReference.ViewModels;

namespace ThaiReference.Views;

public partial class ConsonantClustersView : ContentPage
{
    private int _scrollSyncGeneration;

    public ConsonantClustersView(ConsonantClustersViewModel vm)
    {
        BindingContext = vm;
        InitializeComponent();
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(ConsonantClustersViewModel.HasVisibleSamplesStrip))
                UpdateSamplesPanelMaxHeight();
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SyncActivePaneFromCarousel();
    }

    private void OnClusterCarouselScrolled(object? sender, ItemsViewScrolledEventArgs e)
    {
        if (e.CenterItemIndex < 0 || BindingContext is not ConsonantClustersViewModel vm)
            return;

        var index = e.CenterItemIndex;
        var generation = ++_scrollSyncGeneration;
        Dispatcher.DispatchAsync(async () =>
        {
            await Task.Yield();
            if (generation != _scrollSyncGeneration || index >= vm.TypePanes.Count)
                return;

            vm.ActivatePane(vm.TypePanes[index]);
        });
    }

    private void OnClusterCarouselCurrentItemChanged(object? sender, CurrentItemChangedEventArgs e)
    {
        if (e.CurrentItem is ClusterTypePaneVm pane && BindingContext is ConsonantClustersViewModel vm)
            vm.ActivatePane(pane);
    }

    private void SyncActivePaneFromCarousel()
    {
        if (BindingContext is not ConsonantClustersViewModel vm || vm.TypePanes.Count == 0)
            return;

        if (ClusterCarousel.CurrentItem is ClusterTypePaneVm pane)
            vm.ActivatePane(pane);
        else
            vm.ActivatePane(vm.TypePanes[Math.Clamp(ClusterCarousel.Position, 0, vm.TypePanes.Count - 1)]);
    }

    private void OnClusterContentSizeChanged(object? sender, EventArgs e) =>
        UpdateSamplesPanelMaxHeight();

    private void UpdateSamplesPanelMaxHeight()
    {
        if (ClusterContentRoot.Height <= 0)
            return;
        SamplesScroll.MaximumHeightRequest = ClusterContentRoot.Height * 0.7;
    }
}
