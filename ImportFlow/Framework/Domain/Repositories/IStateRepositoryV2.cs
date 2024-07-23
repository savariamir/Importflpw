using ImportFlow.Events;

namespace ImportFlow.Framework.Domain.Repositories;

public interface IStateRepository<in TEvent> where TEvent : ImportEvent
{
    Task AddAsync(State state);
    Task<IEnumerable<State>> GetAsync(Guid correlationId);
    Task PublishingAsync(TEvent @event);    
    Task SucceedAsync(TEvent @event);
    Task FailedAsync(TEvent @event, string errorMessage);
}