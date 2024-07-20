using ImportFlow.Domain.Repositories;
using ImportFlow.Events;
using MassTransit;

namespace ImportFlow.Consumers;

public abstract class AggregatorConsumer<TEvent>(IConsumer<TEvent> consumer, IStateRepository<TEvent> repository)
    : IConsumer<TEvent>
    where TEvent : ImportEvent
{
    public async Task Consume(ConsumeContext<TEvent> context)
    {
        try
        {
            await consumer.Consume(context);
            await repository.SucceedAsync(context.Message);
        }
        catch (Exception e)
        {
            await repository.FailedAsync(context.Message, e.Message);
            throw;
        }
    }
}