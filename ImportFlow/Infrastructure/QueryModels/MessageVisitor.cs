using ImportFlow.Domain;

namespace ImportFlow.Infrastructure.QueryModels;

public class MessageVisitor : IVisitor
{
    private HashSet<string> _messages { get; set; }

    public void Visit(ImportFlowQueryModel importFlowQuery)
    {
        _messages = [];
        importFlowQuery.State?.Accept(this);
        importFlowQuery.Messages = _messages;
    }

    public void Visit(StateQueryModel state)
    {
        if (state.Name == StepsName.DateExport)
        {
            return;
        }

        var messages = state.Events?
            .Where(p => p.FailedEvents != null)
            .SelectMany(p => p.FailedEvents?.Select(failedEvent => failedEvent.ErrorMessage)
                             ?? Array.Empty<string>()) ?? Array.Empty<string>();

        foreach (var message in messages)
        {
            _messages.Add(message);
        }

        if (state.NextStates is null)
        {
            return;
        }


        foreach (var nextState in state.NextStates)
        {
            nextState.Accept(this);
        }
    }

    public void Visit(EventQueryModel eventQueryModel)
    {
        if (eventQueryModel.FailedEvents is null)
        {
            return;
        }

        foreach (var failedEvent in eventQueryModel.FailedEvents)
        {
            if (failedEvent.ErrorMessage != null)
            {
                _messages.Add(failedEvent.ErrorMessage);
            }
        }
    }
}