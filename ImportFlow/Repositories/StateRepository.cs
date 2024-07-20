using System.Collections.Concurrent;
using ImportFlow.Domain;
using ImportFlow.Domain.Repositories;
using ImportFlow.Events;

namespace ImportFlow.Repositories;

public class StateRepository<TEvent> : IStateRepository<TEvent> where TEvent : ImportEvent
{
    private readonly ConcurrentDictionary<(Guid CorrelationId, Guid CausationId), State<TEvent>> _states = new();

    public Task AddAsync(State<TEvent> state)
    {
        var added = _states.TryAdd((state.CorrelationId, state.CausationId), state);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<State<TEvent>>> GetAsync(Guid correlationId)
    {
        var result = _states
            .Where(kvp => kvp.Key.CorrelationId == correlationId)
            .Select(kvp => kvp.Value);

        return Task.FromResult(result);
    }

    public Task PublishedAsync(TEvent message)
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
}