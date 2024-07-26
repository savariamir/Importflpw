namespace ImportFlow.Domain.Repositories;

public interface IImportFlowRepository
{
    Task AddAsync(ImportProcess importProcessProcess);
    
    Task<IEnumerable<ImportProcess>> GatAllAsync();
    
    Task<ImportProcess> GatByIdAsync(Guid importFlowProcessId);   
}