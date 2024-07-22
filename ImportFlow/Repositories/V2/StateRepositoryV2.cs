using System.Collections.Concurrent;
using ImportFlow.Domain.ModelsV2;
using ImportFlow.Domain.Repositories.V2;
using ImportFlow.Events;

namespace ImportFlow.Repositories.V2;

public class StateRepositoryV2<TEvent> : IStateRepositoryV2<TEvent> where TEvent : ImportEvent
{
    private readonly ConcurrentDictionary<(Guid CorrelationId, Guid CausationId), StateV2> _states = new();
    public Task AddAsync(StateV2 state)
    {
        var added = _states.TryAdd((state.CorrelationId, state.CausationId), state);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<StateV2>> GetAsync(Guid correlationId)
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