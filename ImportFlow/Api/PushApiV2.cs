using ImportFlow.Events;
using ImportFlow.Framework;
using ImportFlow.Framework.Domain;
using ImportFlow.Framework.Domain.Repositories;
using MassTransit;

namespace ImportFlow.Api;

public class PushApi(
    IStateRepository<ImportEvent> repository,
    ImportFlowService importFlowService,
    IBus bus)
{
    public async Task StartAsync()
    {
        var correlationId = Guid.NewGuid();
        var info = new ImportFlowProcessInfo
        {
            PlatformId = 1,
            SupplierId = 21,
            FilesCount = 4,
            CorrelationId = correlationId,
        };

        await importFlowService.StartAsync(info);
        //


        for (var i = 0; i < 4; i++)
        {
            var @event = new SupplierFilesDownloaded
            {
                CorrelationId = correlationId,
                CausationId = correlationId,
                Number = i + 1
            };
            // ...

            await repository.PublishingAsync(@event);
            await bus.Publish(@event);
        }
    }
}