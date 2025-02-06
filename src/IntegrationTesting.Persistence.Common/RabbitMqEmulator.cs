using DotNet.Testcontainers.Containers;
using Testcontainers.RabbitMq;

namespace IntegrationTesting.Persistence.Common;

/// <summary>
///     A container for running RabbitMQ for integration testing.
/// </summary>
public class RabbitMqEmulator
{
    private const string DockerImageName = "rabbitmq:3-management";

    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithImage(DockerImageName)
        .WithPortBinding(5672, true)
        .WithPortBinding(15672, true)
        .Build();

    public string GetConnectionString()
    {
        if (!IsRunning())
        {
            throw new InvalidOperationException(
                "RabbitMQ container must be started before getting the connection string.");
        }

        return $"amqp://guest:guest@{_rabbitMqContainer.Hostname}:{_rabbitMqContainer.GetMappedPublicPort(5672)}";
    }

    private bool IsRunning()
    {
        return _rabbitMqContainer.State == TestcontainersStates.Running;
    }

    public async Task StartAsync()
    {
        await _rabbitMqContainer.StartAsync();
    }

    public async Task StopAsync()
    {
        await _rabbitMqContainer.DisposeAsync();
    }
}