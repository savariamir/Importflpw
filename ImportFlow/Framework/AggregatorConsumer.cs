using ImportFlow.Consumers;
using ImportFlow.Events;
using ImportFlow.Framework.Domain.Repositories;
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