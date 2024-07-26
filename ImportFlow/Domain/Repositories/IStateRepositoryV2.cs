using ImportFlow.Events;

namespace ImportFlow.Domain.Repositories;

public interface IStateRepository<in TEvent> where TEvent : ImportEvent
{
    Task AddAsync(State state);
    Task<IEnumerable<State>> GetAsync(Guid correlationId);
    Task AddEventAsync(TEvent @event);
    Task SucceedEventAsync(TEvent @event);
    Task FailedEventAsync(TEvent @event, string errorMessage);
    Task FinishState(Guid correlationId, Guid causationId);
}