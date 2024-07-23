using ImportFlow.Events;
using ImportFlow.Framework;
using ImportFlow.Framework.Domain;
using ImportFlow.Framework.Domain.Repositories;
using MassTransit;
using State = ImportFlow.Framework.Domain.State;

namespace ImportFlow.Consumers;

public class TransformationConsumer(IStateRepository<ImportEvent> repository)
    : IMessageConsumer<InitialLoadFinished>
{
    public async Task Consume(ConsumeContext<InitialLoadFinished> context)
    {
        var random = new Random();
        var number = random.Next(1, 10);

     
        var causationId = context.Message.EventId;
        
        var state = State.Create(
            StepsName.Transformation,
            context.Message.CorrelationId,
            causationId, 3);

        await repository.AddAsync(state);
        
        if (number == 1)
        {
            throw new Exception($"Something went wrong in Transformation {DateTime.Now.TimeOfDay}");
        }
        
        await Task.Delay(5000);

        for (var i = 0; i < 3; i++)
        {
            var @event = new TransformationFinished
            {
                CausationId = causationId,
                CorrelationId = context.Message.CorrelationId,
                Number = i + 1
            };
            // ...

            await repository.PublishingAsync(@event);
            await context.Publish(@event);
        }
    }
}