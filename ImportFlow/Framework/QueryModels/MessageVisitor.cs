using ImportFlow.Framework.Domain;

namespace ImportFlow.Framework.QueryModels;

public class LogVisitor : IVisitor
{
    private HashSet<string> _messages { get; set; }

    public void Visit(ImportFlowQueryModel importFlow)
    {
        _messages = [];
        importFlow.State?.Accept(this);
        importFlow.Messages = _messages;
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