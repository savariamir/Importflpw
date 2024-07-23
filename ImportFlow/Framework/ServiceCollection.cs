using ImportFlow.Events;
using ImportFlow.Framework.Domain.Repositories;
using ImportFlow.Framework.Repositories;

namespace ImportFlow.Framework;

public static class ServiceCollection
{
    public static IServiceCollection AddImportFlowServices(this IServiceCollection services)
    {
        services.AddTransient<MessageSender>();

        services.AddSingleton<IImportFlowRepository, ImportFlowRepository>();
        services.AddSingleton<IStateRepository<ImportEvent>, StateRepository<ImportEvent>>();

        services.AddScoped<ImportFlowService>();


        return services;
    }
}