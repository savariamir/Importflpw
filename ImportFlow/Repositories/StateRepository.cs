using System.Collections.Concurrent;
using ImportFlow.Domain;
using ImportFlow.Domain.Repositories;
using ImportFlow.Events;

namespace ImportFlow.Repositories;

public class StateRepository<TEvent> : IStateRepository<TEvent> where TEvent: ImportEvent
{
    private readonly ConcurrentDictionary<Guid, State<TEvent>> _states = new();
    
    public Task AddAsync(State<TEvent> state)
    {
        _states[state.CausationId] = state;
        return Task.CompletedTask;
    }
    
    public Task<State<TEvent>> GetAsync(Guid causationId)
    {
        if (_states.TryGetValue((causationId), out var state))
        {
            return Task.FromResult(state);
        }
        return Task.FromResult<State<TEvent>>(null);
    }
    
    public Task PublishedAsync(TEvent message)
    {
        if (_states.TryGetValue((message.CausationId), out var state))
        {
            state.Published(message);
        }

        return Task.CompletedTask;
    }

    public Task SucceedAsync(TEvent message)
    {
        if (_states.TryGetValue((message.CausationId), out var processInfo))
        {
            processInfo.Finished(message);
        }

        return Task.CompletedTask;
    }

    public Task FailedAsync(TEvent message, string errorMessage)
    {
        if (_states.TryGetValue((message.CausationId), out var processInfo))
        {
            processInfo.Failed(message, errorMessage);
        }

        return Task.CompletedTask;
    }
}