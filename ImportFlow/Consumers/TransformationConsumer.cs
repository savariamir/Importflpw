using ImportFlow.Domain;
using ImportFlow.Domain.ModelsV2;
using ImportFlow.Domain.Repositories.V2;
using ImportFlow.Events;
using MassTransit;

namespace ImportFlow.Consumers;

public class TransformationConsumer(IStateRepositoryV2<ImportEvent> repository)
    : IMessageConsumer<InitialLoadFinished>
{
    public async Task Consume(ConsumeContext<InitialLoadFinished> context)
    {
        var random = new Random();
        var number = random.Next(1, 10);

     
        var causationId = context.Message.EventId;
        
        var state = StateV2.Create(
            StepsName.Transformation,
            context.Message.CorrelationId,
            causationId, 3);

        await repository.AddAsync(state);
        
        if (number == 1)
        {
            throw new Exception($"Something went wrong in Transformation {DateTime.Now.TimeOfDay}");
        }
        
        await Task.Delay(10000);

        for (var i = 0; i < 3; i++)
        {
            var @event = new TransformationFinished
            {
                CausationId = causationId,
                CorrelationId = context.Message.CorrelationId,
                Number = i + 1
            };
            // ...

            await repository.PublishedAsync(@event);
            await context.Publish(@event);
        }
    }
}