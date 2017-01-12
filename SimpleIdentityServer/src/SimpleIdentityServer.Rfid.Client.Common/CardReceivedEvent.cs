using Prism.Events;

namespace SimpleIdentityServer.Rfid.Client.Common
{
    public class CardReceivedEvent : PubSubEvent<CardInformation>
    {
    }
}
