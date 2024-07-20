namespace ImportFlow.Events;

public class TransformationFinished: ImportEvent
{
    public IEnumerable<int> ServiceIds { get; set; }
}