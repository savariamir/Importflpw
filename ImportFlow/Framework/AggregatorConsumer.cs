using ImportFlow.Domain.Repositories;
using ImportFlow.Events;
using MassTransit;

namespace ImportFlow.Framework;

public class AggregatorConsumer<TEvent>(IMessageConsumer<TEvent> consumer, IStateRepository<ImportEvent> repository)
    : IConsumer<TEvent>
    where TEvent : ImportEvent
{
    public async Task Consume(ConsumeContext<TEvent> context)
    {
        try
        {
            // await repository.StartedAsync(context.Message);
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