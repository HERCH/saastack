using Common;
using Common.Extensions;
using Common.Recording;
using Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Common.Recording;

/// <summary>
/// Implementación de <see cref="IMetricReporter"/> para entornos on-premises
/// </summary>
public class LocalMetricReporter : IMetricReporter
{
    private readonly ILogger _logger;

    public LocalMetricReporter(IDependencyContainer container)
    {
        _logger = container.GetRequiredService<ILogger>();
    }

    public void Measure(ICallContext? context, string eventName, Dictionary<string, object>? additional = null)
    {
        var properties = additional ?? new Dictionary<string, object>();
        if (context.Exists())
        {
            properties.Add("CallId", context.CallId);
            properties.Add("CallerId", context.CallerId);
        }

        // Log métricas como información estructurada
        _logger.LogInformation(
            "[METRIC] {EventName} - Details: {@Metrics}",
            eventName,
            properties
        );

        // Opcional: Enviar a sistema de métricas local (ej: Prometheus, InfluxDB)
    }
}