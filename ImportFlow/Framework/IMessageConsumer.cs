using ImportFlow.Events;
using MassTransit;

namespace ImportFlow.Framework;

public interface IMessageConsumer<in T> where T : ImportEvent
{
    Task Consume(ConsumeContext<T> context);
}