using ImportFlow.Domain.ModelsV2;
using ImportFlow.Events;

namespace ImportFlow.Domain.Repositories.V2;

public interface IStateRepositoryV2<in TEvent> where TEvent : ImportEvent
{
    Task AddAsync(StateV2 state);
    Task<IEnumerable<StateV2>> GetAsync(Guid correlationId);
    Task PublishedAsync(TEvent @event);
    Task SucceedAsync(TEvent @event);
    Task FailedAsync(TEvent @event, string errorMessage);
}