using ImportFlow.Events;
using MassTransit;

namespace ImportFlow.Consumers;

public interface IMessageConsumer<in T> where T : ImportEvent
{
    Task Consume(ConsumeContext<T> context);
}