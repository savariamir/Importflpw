using ImportFlow.Domain;
using ImportFlow.Domain.ModelsV2;
using ImportFlow.Domain.Repositories.V2;
using ImportFlow.Events;
using ImportFlow.QueryModels;
using MassTransit;

namespace ImportFlow.Api;

public class PushApiV2(
    IImportFlowRepositoryV2 importFlowRepository,
    IStateRepositoryV2<SupplierFilesDownloaded> downloadStepRepository,
    IBus bus)
{
    public async Task StartAsync()
    {
        var info = new ImportFlowProcessInfo
        {
            PlatformId = 1,
            SupplierId = 21,
            FilesCount = 4,
            CorrelationId = Guid.NewGuid(),
        };

        await StartAsync(info);
        //


        for (var i = 0; i < 4; i++)
        {
            var @event = new SupplierFilesDownloaded
            {
                CorrelationId = info.CorrelationId,
                CausationId = info.CorrelationId,
                Number = i + 1
            };
            // ...

            await downloadStepRepository.PublishedAsync(@event);
            await bus.Publish(@event);
        }
    }

    public async Task<IEnumerable<ImportFlowV2>> GetAsync()
    {
        return await importFlowRepository.GatAllAsync();
    }

    public async Task<ImportFlowQueryModel> GatByIdAsync(Guid importFlowProcessId)
    {
        var import = await importFlowRepository.GatByIdAsync(importFlowProcessId);
        return new ImportFlowBuilder().Build(import);
    }

    public async Task<IEnumerable<ImportFlowQueryModel>> GetImportFlowListAsync()
    {
        var imports = await importFlowRepository.GatAllAsync();

        var result = new ImportFlowBuilder().GetImportFlowList(imports);
        return result;
    }

    private async Task StartAsync(ImportFlowProcessInfo info)
    {
        var importFlow = ImportFlowV2.Start(info);
        await importFlowRepository.AddAsync(importFlow);
    }
}