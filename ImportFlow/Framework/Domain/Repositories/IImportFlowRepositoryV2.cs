namespace ImportFlow.Framework.Domain.Repositories;

public interface IImportFlowRepository
{
    Task AddAsync(ImportFlow importFlowProcess);
    
    Task<IEnumerable<ImportFlow>> GatAllAsync();
    
    Task<ImportFlow> GatByIdAsync(Guid importFlowProcessId);   
}