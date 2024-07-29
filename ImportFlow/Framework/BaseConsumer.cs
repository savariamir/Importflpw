using ImportFlow.Domain.Repositories;
using ImportFlow.Events;
using MassTransit;

namespace ImportFlow.Framework;

// public class Something : IFilter<IConsumer>
// {
//     
// }
public class BaseConsumer<TEvent>(IMessageConsumer<TEvent> consumer, IStateRepository<ImportEvent> repository)
    : IConsumer<TEvent>
    where TEvent : ImportEvent
{
    public async Task Consume(ConsumeContext<TEvent> context)
    {
        try
        {
            await consumer.Consume(context);
            await repository.SucceedEventAsync(context.Message);
        }
        catch (Exception e)
        {
            await repository.FailedEventAsync(context.Message, e.Message);
            throw;
        }
    }
}