namespace ImportFlow.Infrastructure.QueryModels;

public interface IVisitor
{
    void Visit(StateQueryModel state);

    void Visit(ImportFlowQueryModel state);

    void Visit(EventQueryModel @event);
}