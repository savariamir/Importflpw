namespace ImportFlow.Domain.Repositories;

public interface IImportFlowRepository
{
    Task StartAsync(ImportProcess importProcessProcess);
    
    Task<IEnumerable<ImportProcess>> GatAllAsync();
    
    Task<ImportProcess> GatByIdAsync(Guid importFlowProcessId);   
}