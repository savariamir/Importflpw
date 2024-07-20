using ImportFlow.Domain;
using ImportFlow.Domain.Repositories;
using ImportFlow.Events;

namespace ImportFlow.Api;

public class PushApi(IImportFlowRepository repository, IStateRepository<SupplierFilesDownloaded> stateRepository)
{
    public async Task StartAsync()
    {
        var info = new ImportFlowProcessInfo
        {
            PlatformId = 1,
            SupplierId = 21,
            FilesCount = 4,
            CorrelationId = Guid.NewGuid()
        };

        await StartAsync(info);
        //


        for (var i = 0; i < 4; i++)
        {
            var @event = new SupplierFilesDownloaded
            {
                CausationId = info.CorrelationId,
                CorrelationId = info.CorrelationId,
                Number = i + 1
            };
            // ...

            await stateRepository.PublishedAsync(@event);
        }
    }

    public async Task<IEnumerable<ImportFlowProcess>> GetAsync()
    {
        return await repository.GatAllAsync();
    }

    private async Task StartAsync(ImportFlowProcessInfo info)
    {
        var importFlow = ImportFlowProcess.Start(info);
        await repository.AddAsync(importFlow);
    }
}