using ImportFlow.Domain.ModelsV2;

namespace ImportFlow.Domain.Repositories.V2;

public interface IImportFlowRepositoryV2
{
    Task AddAsync(ImportFlowV2 importFlowProcess);
    
    Task<IEnumerable<ImportFlowV2>> GatAllAsync();
    
    Task<ImportFlowV2> GatByIdAsync(Guid importFlowProcessId);   
}