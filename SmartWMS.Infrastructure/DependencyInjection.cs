namespace Microsoft.Extensions.DependencyInjection;

using SmartWMS.Application.Features.Anomaly.DataProviders;
using SmartWMS.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Data Provider Layer (Analytic Access)
        services.AddScoped<IAnomalyDataProvider, AnomalyDataProvider>();

        // Repositories & UoW (Şimdilik interface'ler tanımlı, concrete'ler DB katmanıyla gelecek)
        // services.AddScoped<IShelfRepository, ShelfRepository>();

        return services;
    }
}
