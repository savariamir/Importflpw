namespace ImportFlow.Domain;

public enum ImportStatus
{
    Processing = 1,
    Completed = 2,
    PartiallyFailed = 3,
    Failed = 4
}