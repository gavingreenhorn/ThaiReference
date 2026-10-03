using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ThaiReference.Messages
{
    public sealed class RomanizationOptionChanged(string value) : ValueChangedMessage<string>(value)
    {
    }
}
