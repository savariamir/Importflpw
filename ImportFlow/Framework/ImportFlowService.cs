using ImportFlow.Events;
using ImportFlow.Framework.Domain;
using ImportFlow.Framework.Domain.Repositories;
using ImportFlow.Framework.QueryModels;

namespace ImportFlow.Framework;

public class ImportFlowService(
    IImportFlowRepository importFlowRepository,
    IStateRepository<ImportEvent> downloadStepRepository)
{
    public async Task<IEnumerable<Framework.Domain.ImportFlow>> GetAsync()
    {
        return await importFlowRepository.GatAllAsync();
    }

    public async Task<ImportFlowQueryModel> GatByIdAsync(Guid importFlowProcessId)
    {
        var import = await importFlowRepository.GatByIdAsync(importFlowProcessId);
        return new QueryModelBuilder().Build(import);
    }

    public async Task<IEnumerable<ImportFlowQueryModel>> GetImportFlowListAsync()
    {
        var imports = await importFlowRepository.GatAllAsync();
        var result = new QueryModelBuilder().GetImportFlowList(imports);
        return result;
    }

    public async Task StartAsync(ImportFlowProcessInfo info)
    {
        var importFlow = Framework.Domain.ImportFlow.Start(info);
        await importFlowRepository.AddAsync(importFlow);
    }
}