using CommunityToolkit.Mvvm.Messaging;
using ThaiReference.Messages;

namespace ThaiReference.Contracts;

internal interface IRomanizationReceiver : IRecipient<RomanizationOptionChanged>
{
    public string RomanizationOption { get; set; }
}
