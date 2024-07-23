using ImportFlow.Application;
using ImportFlow.Domain.Repositories;
using ImportFlow.Events;
using ImportFlow.Infrastructure.InMemoryPersist;

namespace ImportFlow.Infrastructure.Configs;

public static class ServiceCollection
{
    public static IServiceCollection AddImportFlowServices(this IServiceCollection services)
    {
        services.AddTransient<MessageSender>();

        services.AddSingleton<IImportFlowRepository, InMemoryImportFlowRepository>();
        services.AddSingleton<IStateRepository<ImportEvent>, InMemoryStateRepository<ImportEvent>>();

        services.AddScoped<ImportFlowService>();


        return services;
    }
}