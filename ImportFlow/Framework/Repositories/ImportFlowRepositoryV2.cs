using ImportFlow.Events;
using ImportFlow.Framework.Domain.Repositories;

namespace ImportFlow.Framework.Repositories;

public class ImportFlowRepository(
    IStateRepository<ImportEvent> repository
) : IImportFlowRepository
{
    private readonly List<Domain.ImportFlow> _database = new();

    public async Task AddAsync(Domain.ImportFlow import)
    {
        _database.Add(import);
        await repository.AddAsync(import.DownloadedFilesState);
    }

    public async Task<IEnumerable<Domain.ImportFlow>> GatAllAsync()
    {
        foreach (var importFlowProcess in _database)
        {
            var states = await repository.GetAsync(importFlowProcess.Id);
            importFlowProcess.Set(states.ToList());
        }

        return _database.OrderByDescending(p => p.CreateAt);
    }

    public async Task<Domain.ImportFlow> GatByIdAsync(Guid importFlowProcessId)
    {
        var first = _database.FirstOrDefault(p => p.Id == importFlowProcessId);
        if (first is null)
        {
            return null;
        }

        var states = await repository.GetAsync(first.Id);
        first.Set(states.ToList());
        return first;
    }
}