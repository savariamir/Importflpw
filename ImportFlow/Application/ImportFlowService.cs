using ImportFlow.Domain;
using ImportFlow.Domain.Repositories;
using ImportFlow.Infrastructure.Query;
using ImportFlow.Infrastructure.QueryModels;

namespace ImportFlow.Application;

public class ImportFlowService(IImportFlowRepository importFlowRepository)
{
    public async Task<IEnumerable<ImportFlowProcess>> GetAsync()
    {
        return await importFlowRepository.GatAllAsync();
    }

    public async Task<ImportFlowQueryModel> GatByIdAsync(Guid importFlowProcessId)
    {
        var import = await importFlowRepository.GatByIdAsync(importFlowProcessId);
        return QueryModelBuilder.Build(import);
    }

    public async Task<IEnumerable<ImportFlowQueryModel>> GetImportFlowListAsync()
    {
        var imports = await importFlowRepository.GatAllAsync();
        var result = QueryModelBuilder.GetImportFlowList(imports);
        return result;
    }

    public async Task StartAsync(ImportFlowProcessInfo info)
    {
        var importFlow = ImportFlowProcess.Start(info);
        await importFlowRepository.AddAsync(importFlow);
    }
}