using Common;
using Common.Extensions;
using Common.Recording;
using Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Common.Recording;

/// <summary>
/// Implementación de <see cref="ICrashReporter"/> para entornos on-premises
/// </summary>
public class LocalCrashReporter : ICrashReporter
{
    private readonly ILogger _logger;

    // Constructor para inyección directa de dependencias
    public LocalCrashReporter(ILogger logger)
    {
        _logger = logger;
    }

    // Constructor compatible con contenedor de dependencias
    public LocalCrashReporter(IDependencyContainer container)
    {
        _logger = container.GetRequiredService<ILogger>();
    }

    public void Crash(ICallContext? call, CrashLevel level, Exception exception, string messageTemplate, object[] templateArgs)
    {
        // Construir mensaje estructurado
        var properties = new Dictionary<string, object>
        {
            { "CrashLevel", level.ToString() },
            { "MessageTemplate", messageTemplate },
            { "TemplateArgs", string.Join(", ", templateArgs.Select(arg => arg.ToString())) }
        };

        if (call.Exists())
        {
            properties.Add("CallId", call.CallId);
            properties.Add("CallerId", call.CallerId);
        }

        // Ejemplo de log estructurado para on-premises
        _logger.LogError(
            exception,
            "[CRASH] {MessageTemplate} - Level: {CrashLevel}, Args: {TemplateArgs}",
            messageTemplate,
            level,
            properties["TemplateArgs"]
        );

        // Opcional: Integrar con sistema de monitoreo local (ej: escribir en BD, enviar a API interna)
    }
}