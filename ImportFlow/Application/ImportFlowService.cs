using ImportFlow.Domain;
using ImportFlow.Domain.Repositories;
using ImportFlow.Events;
using ImportFlow.Infrastructure;
using ImportFlow.Infrastructure.QueryModels;

namespace ImportFlow.Application;

public class ImportMonitoring(IImportFlowRepository importFlowRepository, IStateRepository<ImportEvent> repository)
{
    public async Task<IEnumerable<ImportProcess>> GetAllAsync()
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

    public async Task StartAsync(ImportProcessOptions options)
    {
        var importFlow = ImportProcess.Start(options);
        await importFlowRepository.AddAsync(importFlow);
    }

    public async Task StartStateAsync(string stepName,Guid correlationId, Guid causationId, int totalCount)
    {
        var state = State.Start(stepName, correlationId, causationId, totalCount);
        await repository.AddAsync(state);
    }
    
    public async Task FinishStateAsync(Guid correlationId, Guid causationId)
    {
        await repository.FinishState(correlationId, causationId);
    }
}
