using ImportFlow.Domain.Repositories;
using ImportFlow.Events;
using MassTransit;

namespace ImportFlow.Application;

public class MessagePublisher(IBus bus, IStateRepository<ImportEvent> repository)
{
    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : ImportEvent
    {
        await repository.AddEventAsync(@event);
        await bus.Publish(@event);
    }
}