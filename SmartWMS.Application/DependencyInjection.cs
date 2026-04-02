namespace Microsoft.Extensions.DependencyInjection;

using SmartWMS.Application.Features.Anomaly;
using SmartWMS.Application.Features.Anomaly.Orchestrator;
using SmartWMS.Application.Features.Anomaly.Rules;
using SmartWMS.Application.Features.Anomaly.Rules.Inventory;
using MediatR;
using System.Reflection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR (Command/Query için)
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Decision Intelligence Pipeline (3-Stage Orchestrator)
        services.AddScoped<ICorrelationEngine, CorrelationEngine>();
        services.AddScoped<IConfidenceReconciler, ConfidenceReconciler>();
        services.AddScoped<IExplanationMapper, ExplanationMapper>();
        services.AddScoped<IAnomalyOrchestrator, AnomalyOrchestrator>();
        
        // Central Anomaly Engine
        services.AddScoped<IAnomalyEngine, AnomalyEngine>();

        // Anomaly Rules (Koleksiyon olarak enjekte edilecek)
        services.AddScoped<IAnomalyRule, SuddenShrinkageRule>();

        return services;
    }
}
