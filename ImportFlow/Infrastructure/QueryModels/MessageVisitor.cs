using ImportFlow.Domain;

namespace ImportFlow.Infrastructure.QueryModels;

public class LogVisitor : IVisitor
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
        if (state.Name == StepsName.DateExport || state.Events is null)
        {
            return;
        }

        foreach (var eventQuery in state.Events)
        {
            eventQuery.Accept(this);
        }
    }

    public void Visit(EventQueryModel eventQueryModel)
    {
        eventQueryModel.State?.Accept(this);

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