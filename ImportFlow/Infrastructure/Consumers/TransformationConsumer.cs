using ImportFlow.Application;
using ImportFlow.Domain;
using ImportFlow.Events;
using ImportFlow.Framework;
using MassTransit;

namespace ImportFlow.Infrastructure.Consumers;

public class TransformationConsumer(MessagePublisher messagePublisher, ImportMonitoring monitoring)
    : IMessageConsumer<InitialLoadFinished>
{
    public async Task Consume(ConsumeContext<InitialLoadFinished> context)
    {
        // Start consuming
        var totalEventsCount = 3;
        var stateOptions = new StateOptions
        {
            Name = StepsName.Transformation,
            CorrelationId = context.Message.CorrelationId,
            CausationId = context.Message.EventId,
            TotalCount = totalEventsCount,
            HaveEvents = true
        };
        
        await monitoring.AddStateAsync(stateOptions);

        // Processing
        await Task.Delay(50000);


        var random = new Random();
        var number = random.Next(1, 10);
        if (number == 1)
        {
            throw new Exception($"Something went wrong in Transformation {DateTime.Now.TimeOfDay}");
        }

        for (var i = 0; i < totalEventsCount; i++)
        {
            var @event = new TransformationFinished
            {
                CorrelationId = context.Message.CorrelationId,
                CausationId = context.Message.EventId,
                Number = i + 1
            };
            // ...

            await messagePublisher.PublishAsync(@event);
        }
    }
}