namespace ImportFlow.Framework.QueryModels;

public interface IVisitor
{
    void Visit(StateQueryModel state);

    void Visit(ImportFlowQueryModel state);

    void Visit(EventQueryModel @event);
}