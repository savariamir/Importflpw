namespace ImportFlow.Events;

public class TransformationFinished: ImportEvent
{
    public int ServiceId { get; set; }

    public int SubServiceId { get; set; }
}