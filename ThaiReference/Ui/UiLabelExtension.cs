using ThaiReference.Converters;

namespace ThaiReference.Ui;

/// <summary>Binds to <see cref="UiLanguageState"/> and resolves <see cref="UiLabels"/> by id.</summary>
[ContentProperty(nameof(Id))]
[AcceptEmptyServiceProvider]
public sealed class UiLabelExtension : IMarkupExtension<BindingBase>
{
	public string Id { get; set; } = "";

	public BindingBase ProvideValue(IServiceProvider serviceProvider) =>
		new Binding(nameof(UiLanguageState.Language), source: UiLanguageState.Instance)
		{
			Converter = UiLabelConverter.Shared,
			ConverterParameter = Id,
		};

	object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}
