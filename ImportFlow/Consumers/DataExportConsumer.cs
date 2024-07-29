using ImportFlow.Application;
using ImportFlow.Domain;
using ImportFlow.Events;
using ImportFlow.Framework;
using MassTransit;

namespace ImportFlow.Consumers;

public class DataExportConsumer(ImportMonitoring monitoring) : IMessageConsumer<TransformationFinished>
{
    public async Task Consume(ConsumeContext<TransformationFinished> context)
    {
        // Start consuming
        var totalEventsCount = 0;

        var stateOptions = new StateOptions
        {
            Name = StepsName.DateExport,
            CorrelationId = context.Message.CorrelationId,
            CausationId = context.Message.EventId,
            TotalCount = totalEventsCount,
            HasEvents = false
        };

        await monitoring.AddStateAsync(stateOptions);
        
        // Processing

        await Task.Delay(1000);

        var random = new Random();
        var number = random.Next(1, 10);
        if (number == 1)
        {
            throw new Exception($"Something went wrong in Data Export {DateTime.Now.TimeOfDay}");
        }


        // Since Data Export is the last state of the flow, we need to finish it.
        await monitoring.FinishStateAsync(context.Message.CorrelationId, context.Message.EventId);
    }
}