using ImportFlow.Application;
using ImportFlow.Domain;
using ImportFlow.Domain.Repositories;
using ImportFlow.Events;
using ImportFlow.Framework;
using MassTransit;

namespace ImportFlow.Api;

public class PushApi(MessagePublisher messagePublisher, ImportMonitoring monitoring)
{
    public async Task StartAsync()
    {
        var correlationId = Guid.NewGuid();
        var totalEventsCount = 4;
        var info = new ImportProcessOptions
        {
            StepName = StepsName.PushApi,
            PlatformId = 1,
            SupplierId = 21,
            TotalEventsCount = totalEventsCount,
            CorrelationId = correlationId,
        };

        await monitoring.StartAsync(info);
        //


        for (var i = 0; i < totalEventsCount; i++)
        {
            var @event = new SupplierFilesDownloaded
            {
                CorrelationId = correlationId,
                CausationId = correlationId,
                Number = i + 1
            };
            // ...

            await messagePublisher.PublishAsync(@event);
        }
    }
}