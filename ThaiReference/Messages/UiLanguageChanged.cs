using CommunityToolkit.Mvvm.Messaging.Messages;
using ThaiReference.Ui;

namespace ThaiReference.Messages;

public sealed class UiLanguageChanged(UiLanguage value) : ValueChangedMessage<UiLanguage>(value)
{
}
