using ImportFlow.Domain.Repositories.V2;
using ImportFlow.Events;
using MassTransit;

namespace ImportFlow.Consumers;

public class AggregatorConsumer<TEvent>(IMessageConsumer<TEvent> consumer, IStateRepositoryV2<ImportEvent> repository)
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