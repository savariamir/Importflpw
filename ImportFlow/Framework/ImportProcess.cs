using ImportFlow.Framework.Domain;

namespace ImportFlow.Framework;

public static class ImportProcess
{
    public static string GetNextName(string stateName)
    {
        return stateName switch
        {
            StepsName.SupplierFiles =>  StepsName.InitialLoad,
            StepsName.InitialLoad =>  StepsName.Transformation,
            StepsName.Transformation =>  StepsName.DateExport,
            StepsName.DateExport =>  string.Empty,
            _ => throw new InvalidOperationException()
        };
    }
}