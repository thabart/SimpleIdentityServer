using Moq;
using SimpleBus.Core;
using Xunit;

namespace SimpleBus.RabbitMq.Tests
{
    public class RabbitMqTests
    {
        [Fact]
        public void When_Publish_Event_Then_Event_Is_Handle()
        {
            var evtHandlerStoreStub = new Mock<IEvtHandlerStore>();
            var rabbitMqBus = new RabbitMqBus(evtHandlerStoreStub.Object);
            rabbitMqBus.Init();

            rabbitMqBus.Publish(new FakeEvent());


        }
    }
}
