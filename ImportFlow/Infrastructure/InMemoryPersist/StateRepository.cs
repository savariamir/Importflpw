using System.Collections.Concurrent;
using ImportFlow.Domain;
using ImportFlow.Domain.Repositories;
using ImportFlow.Events;

namespace ImportFlow.Infrastructure.InMemoryPersist;

public class InMemoryStateRepository<TEvent> : IStateRepository<TEvent> where TEvent : ImportEvent
{
    private readonly ConcurrentDictionary<(Guid CorrelationId, Guid CausationId), State> _states = new();
    public Task AddAsync(State state)
    {
        _states.TryAdd((state.CorrelationId, state.CausationId), state);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<State>> GetAsync(Guid correlationId)
    {
        var result = _states
            .Where(kvp => kvp.Key.CorrelationId == correlationId)
            .Select(kvp => kvp.Value);

        return Task.FromResult(result);
    }

    public Task PublishingAsync(TEvent message)
    {
        if (_states.TryGetValue((message.CorrelationId, message.CausationId), out var state))
        {
            state.Published(message);
        }

        return Task.CompletedTask;
    }

    public Task SucceedAsync(TEvent message)
    {
        if (_states.TryGetValue((message.CorrelationId, message.CausationId), out var processInfo))
        {
            processInfo.Finished(message);
        }

        return Task.CompletedTask;
    }

    public Task FailedAsync(TEvent message, string errorMessage)
    {
        if (_states.TryGetValue((message.CorrelationId, message.CausationId), out var processInfo))
        {
            processInfo.Failed(message, errorMessage);
        }

        return Task.CompletedTask;
    }

    public async Task StartedAsync(TEvent @event)
    {
        throw new NotImplementedException();
    }
}