using ImportFlow.Domain;

namespace ImportFlow.QueryModels;

public class StatusVisitor : IVisitor
{
    public void Visit(ImportFlowQueryModel importFlow)
    {
        importFlow.State?.Accept(this);
        importFlow.Status = importFlow.State.Status;
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

        var statuses = state.Events.Select(p => p.State?.Status).ToList();

        if (statuses.Contains(ImportStatus.Processing.ToString()))
        {
            state.Status = ImportStatus.Processing.ToString();
        }
        else if (statuses.All(s => s == ImportStatus.Completed.ToString()))
        {
            state.Status = ImportStatus.Completed.ToString();
        }

        else if (statuses.All(s => s == ImportStatus.Failed.ToString()))
        {
            state.Status = ImportStatus.Failed.ToString();
        }
        else
        {
            state.Status = ImportStatus.PartiallyFailed.ToString();
        }
    }


    public void Visit(EventQueryModel eventQueryModel)
    {
        eventQueryModel.State?.Accept(this);
    }
}

public class MessageVisitor : IVisitor
{
    private HashSet<string> _messages { get; set; }

    public IEnumerable<string> GetMessages => _messages;

    public void Visit(ImportFlowQueryModel importFlow)
    {
        _messages = [];
        importFlow.State?.Accept(this);
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