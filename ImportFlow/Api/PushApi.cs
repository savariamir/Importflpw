using ImportFlow.Application;
using ImportFlow.Domain;
using ImportFlow.Events;

namespace ImportFlow.Api;

public class PushApi(MessagePublisher messagePublisher, ImportMonitoring monitoring)
{
    public async Task StartAsync()
    {
        var correlationId = Guid.NewGuid();
        var totalEventsCount = 4;

        var importOptions = new ImportProcessOptions
        {
            PlatformId = 1,
            SupplierId = 21,
            CorrelationId = correlationId,
            Transitions = new Dictionary<string, string>
            {
                {StepsName.PushApi, StepsName.InitialLoad},
                {StepsName.InitialLoad, StepsName.Transformation},
                {StepsName.Transformation, StepsName.DateExport}
            }
        };

        var initialStateOptions = new StateOptions
        {
            Name = StepsName.PushApi,
            CorrelationId = correlationId,
            CausationId = correlationId,
            TotalCount = totalEventsCount,
            HasEvents = true
        };

        await monitoring.StartAsync(importOptions, initialStateOptions);
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