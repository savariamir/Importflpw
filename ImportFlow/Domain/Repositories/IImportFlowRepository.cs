namespace ImportFlow.Domain.Repositories;

public interface IImportFlowRepository
{
    Task AddAsync(ImportFlowProcess importFlowProcess);
    
    Task<IEnumerable<ImportFlowProcess>> GatAllAsync();
}