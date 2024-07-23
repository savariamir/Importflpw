namespace ImportFlow.Domain.Repositories;

public interface IImportFlowRepository
{
    Task AddAsync(ImportFlowProcess importFlowProcessProcess);
    
    Task<IEnumerable<ImportFlowProcess>> GatAllAsync();
    
    Task<ImportFlowProcess> GatByIdAsync(Guid importFlowProcessId);   
}