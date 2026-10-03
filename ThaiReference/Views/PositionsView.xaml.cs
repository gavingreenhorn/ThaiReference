using ThaiReference.ViewModels;

namespace ThaiReference.Views;

public partial class PositionsView : ContentPage
{
	int _scrollSyncGeneration;

	public PositionsView(PositionsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		SyncCurrentItemFromCarousel();
	}

	void OnPositionCarouselScrolled(object? sender, ItemsViewScrolledEventArgs e)
	{
		if (e.CenterItemIndex < 0 || BindingContext is not PositionsViewModel vm)
			return;

		var index = e.CenterItemIndex;
		var generation = ++_scrollSyncGeneration;
		Dispatcher.DispatchAsync(async () =>
		{
			await Task.Yield();
			if (generation != _scrollSyncGeneration || index >= vm.Positions.Count)
				return;

			vm.ActivateItem(vm.Positions[index]);
		});
	}

	void OnPositionCarouselCurrentItemChanged(object? sender, CurrentItemChangedEventArgs e)
	{
		if (e.CurrentItem is PositionWord item && BindingContext is PositionsViewModel vm)
			vm.ActivateItem(item);
	}

	void OnSceneStartInteraction(object? sender, TouchEventArgs e)
	{
		if (sender is not GraphicsView view || view.BindingContext is not PositionWord card)
			return;
		if (e.Touches.Length == 0)
			return;

		var touch = e.Touches[0];
		if (!card.HandleSceneTap(touch.X, touch.Y))
			return;

		view.Invalidate();
		if (BindingContext is PositionsViewModel vm)
			vm.ActivateItem(card);
	}

	void SyncCurrentItemFromCarousel()
	{
		if (BindingContext is not PositionsViewModel vm || vm.Positions.Count == 0)
			return;

		if (PositionCarousel.CurrentItem is PositionWord item)
			vm.ActivateItem(item);
		else
			vm.ActivateItem(vm.Positions[Math.Clamp(PositionCarousel.Position, 0, vm.Positions.Count - 1)]);
	}
}
