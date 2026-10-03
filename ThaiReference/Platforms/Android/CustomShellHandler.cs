using Android.Content;
using Android.OS;
using Android.Views;
using Google.Android.Material.Tabs;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using System.Collections.Specialized;

namespace ThaiReference;

public class CustomShellHandler(Context context) : ShellRenderer(context)
{
	protected override IShellSectionRenderer CreateShellSectionRenderer(ShellSection shellSection) =>
		new NoSwipeShellSectionRenderer(this);

	protected override IShellTabLayoutAppearanceTracker CreateTabLayoutAppearanceTracker(ShellSection shellSection) =>
		new MyTabLayoutAppearanceTracker(this);
}

internal sealed class NoSwipeShellSectionRenderer(IShellContext shellContext) : ShellSectionRenderer(shellContext)
{
	public override Android.Views.View? OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle? savedInstanceState)
	{
		var view = base.OnCreateView(inflater, container, savedInstanceState);
		DisableSectionSwipe();
		view?.Post(DisableSectionSwipe);
		return view;
	}

	protected override void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		base.OnItemsCollectionChanged(sender, e);
		DisableSectionSwipe();
	}

	protected override void SetViewPager2UserInputEnabled(bool value) =>
		base.SetViewPager2UserInputEnabled(false);

	void DisableSectionSwipe() => SetViewPager2UserInputEnabled(false);
}

public class MyTabLayoutAppearanceTracker(IShellContext shellContext) : ShellTabLayoutAppearanceTracker(shellContext)
{
	public override void SetAppearance(TabLayout tabLayout, ShellAppearance appearance)
	{
		base.SetAppearance(tabLayout, appearance);
		tabLayout.TabMode = TabLayout.ModeScrollable;
		tabLayout.TabGravity = TabLayout.GravityFill;
		tabLayout.TabIndicatorAnimationMode = TabLayout.IndicatorAnimationModeElastic;
		tabLayout.SetSelectedTabIndicatorColor(Color.FromArgb("F78C6B").ToInt());
		tabLayout.SetTabTextColors(Colors.Turquoise.ToInt(), Color.FromArgb("F78C6B").ToInt());
	}
}
