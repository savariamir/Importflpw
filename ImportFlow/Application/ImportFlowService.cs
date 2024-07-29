using ImportFlow.Domain;
using ImportFlow.Domain.Repositories;
using ImportFlow.Events;
using ImportFlow.Infrastructure;
using ImportFlow.Infrastructure.QueryModels;

namespace ImportFlow.Application;

public class ImportMonitoring(IImportFlowRepository importFlowRepository, IStateRepository<ImportEvent> stateRepository)
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

    public async Task<IEnumerable<State>> GetStates(Guid correlationId)
    {
        var states = await stateRepository.GetAsync(correlationId);
        return states;
    }

    public async Task StartAsync(ImportProcessOptions options, StateOptions initialStateOptions)
    {
        var importFlow = ImportProcess.NewImport(options);
        var initialState = State.InitiateState(initialStateOptions);

        await importFlowRepository.StartAsync(importFlow);
        await stateRepository.AddAsync(initialState);
    }

    public async Task AddStateAsync(StateOptions options)
    {
        var state = State.AddState(options);
        await stateRepository.AddAsync(state);
    }

    public async Task FinishStateAsync(Guid correlationId, Guid causationId)
    {
        await stateRepository.FinishState(correlationId, causationId);
    }
}