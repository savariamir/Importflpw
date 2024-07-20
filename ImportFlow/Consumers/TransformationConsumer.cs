using ImportFlow.Domain;
using ImportFlow.Domain.Repositories;
using ImportFlow.Events;
using MassTransit;

namespace ImportFlow.Consumers;

public class TransformationConsumer(IStateRepository<TransformationFinished> repository)
    : IMessageConsumer<InitialLoadFinished>
{
    public async Task Consume(ConsumeContext<InitialLoadFinished> context)
    {
        var random = new Random();
        var number = random.Next(1, 10);

        if (number == 1)
        {
            throw new Exception("Something went wrong");
        }
        
        var state = Domain.State<TransformationFinished>.Create(
            StepsName.Transformation,
            context.Message.CorrelationId,
            context.Message.CausationId, 3);

        await repository.AddAsync(state);

        for (var i = 0; i < 3; i++)
        {
            var @event = new TransformationFinished
            {
                CausationId = context.Message.EventId,
                CorrelationId = context.Message.CorrelationId,
                Number = i + 1
            };
            // ...

            await repository.PublishedAsync(@event);
            await context.Publish(@event);
        }
    }
}