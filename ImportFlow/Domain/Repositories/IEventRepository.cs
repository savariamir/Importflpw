using ImportFlow.Events;

namespace ImportFlow.Domain.Repositories;

public interface IStateRepository<TEvent> where TEvent: ImportEvent
{
    Task AddAsync(State<TEvent> state);
    Task<State<TEvent>> GetAsync(Guid causationId);
    Task PublishedAsync(TEvent message);
    Task SucceedAsync(TEvent message);
    Task FailedAsync(TEvent message, string errorMessage);
}