using ImportFlow.Domain;
using ImportFlow.Events;

namespace ImportFlow;

public static class ImportProcess
{
    public static string GetTypeName<TEvent>(TEvent @event) where TEvent : ImportEvent
    {
        return @event switch
        {
            SupplierFilesDownloaded => StepsName.SupplierFiles,
            InitialLoadFinished => StepsName.InitialLoad,
            TransformationFinished => StepsName.Transformation,
            DataExported => StepsName.DateExport,
            _ => throw new InvalidOperationException()
        };
    }
}