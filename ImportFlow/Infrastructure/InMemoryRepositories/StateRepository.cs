using System.Collections.Concurrent;
using ImportFlow.Domain;
using ImportFlow.Domain.Repositories;
using ImportFlow.Events;

namespace ImportFlow.Infrastructure.InMemoryRepositories;

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

    public Task AddEventAsync(TEvent message)
    {
        if (_states.TryGetValue((message.CorrelationId, message.CausationId), out var state))
        {
            state.Add(message);
        }

        return Task.CompletedTask;
    }
    
    public Task SucceedEventAsync(TEvent message)
    {
        if (_states.TryGetValue((message.CorrelationId, message.CausationId), out var state))
        {
            state.Succeed(message);
        }

        return Task.CompletedTask;
    }

    public Task FailedEventAsync(TEvent message, string errorMessage)
    {
        if (_states.TryGetValue((message.CorrelationId, message.CausationId), out var state))
        {
            state.Fail(message, errorMessage);
        }

        return Task.CompletedTask;
    }

    public Task FinishState(Guid correlationId, Guid causationId)
    {
        if (_states.TryGetValue((correlationId, causationId), out var state))
        {
            state.Finish();
        }

        return Task.CompletedTask;
    }
}