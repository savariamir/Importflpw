using ImportFlow.Application;
using ImportFlow.Domain;
using ImportFlow.Events;
using ImportFlow.Framework;
using MassTransit;

namespace ImportFlow.Infrastructure.Consumers;

public class InitialLoadConsumer(MessagePublisher messagePublisher, ImportMonitoring monitoring)
    : IMessageConsumer<SupplierFilesDownloaded>
{
    public async Task Consume(ConsumeContext<SupplierFilesDownloaded> context)
    {
        // Start consuming
        var totalEventsCount = 5;
        await monitoring.StartStateAsync(
            StepsName.InitialLoad,
            context.Message.CorrelationId,
            context.Message.EventId,
            totalEventsCount);

        // var random = new Random();
        // var number = random.Next(1, 10);
        // if (number == 1)
        // {
        //     throw new Exception($"Something went wrong in Initial Load {DateTime.Now.TimeOfDay}");
        // }

        // Processing
        await Task.Delay(1000);

        for (var i = 0; i < totalEventsCount; i++)
        {
            var @event = new InitialLoadFinished
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