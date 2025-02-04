using Common;
using Common.Extensions;
using Infrastructure.Persistence.Interfaces;
using RabbitMQ.Client;
using System.Text;
using JetBrains.Annotations;

namespace Infrastructure.Persistence.OnPremises.ApplicationServices;

[UsedImplicitly]
public sealed class RabbitMqStore : IMessageBusStore, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IRecorder _recorder;
    private readonly SemaphoreSlim _syncLock = new(1, 1);
    private readonly Dictionary<string, bool> _declaredExchanges = new();
    private readonly Dictionary<(string Exchange, string Queue), bool> _declaredBindings = new();

    public static RabbitMqStore Create(IRecorder recorder, RabbitMqStoreOptions options)
    {
        var factory = new ConnectionFactory
        {
            HostName = options.HostName,
            UserName = options.UserName,
            Password = options.Password,
            VirtualHost = options.VirtualHost ?? "/",
            DispatchConsumersAsync = true
        };

        // Sincronización forzada para mantener la interfaz síncrona
        var connection = Task.Run(() => factory.CreateConnection()).GetAwaiter().GetResult();
        var channel = Task.Run(() => connection.CreateModel()).GetAwaiter().GetResult();

        return new RabbitMqStore(recorder, connection, channel);
    }

    private RabbitMqStore(IRecorder recorder, IConnection connection, IModel channel)
    {
        _recorder = recorder;
        _connection = connection;
        _channel = channel;
    }

    public async ValueTask DisposeAsync()
    {
        await Task.Run(() =>
        {
            _channel?.Close();
            _connection?.Close();
        });
    }

    public async Task<Result<Error>> SendAsync(string exchangeName, string message, CancellationToken cancellationToken)
    {
        exchangeName.ThrowIfNotValuedParameter(nameof(exchangeName), Resources.AnyStore_MissingTopicName);
        message.ThrowIfNotValuedParameter(nameof(message), Resources.AnyStore_MissingSentMessage);

        try
        {
            await EnsureExchangeDeclaredAsync(exchangeName, cancellationToken);
            var body = Encoding.UTF8.GetBytes(message);

            await PublishMessageAsync(exchangeName, body, cancellationToken);
            return Result.Ok;
        }
        catch (Exception ex)
        {
            _recorder.TraceError(null, ex,
                "Failed to send message: {Message} to exchange: {Exchange}",
                message, exchangeName);
            return ex.ToError(ErrorCode.Unexpected);
        }
    }

    public async Task<Result<Error>> SubscribeAsync(string exchangeName, string queueName, CancellationToken cancellationToken)
    {
        try
        {
            await EnsureExchangeDeclaredAsync(exchangeName, cancellationToken);
            await EnsureQueueDeclaredAsync(queueName, cancellationToken);
            await EnsureBindingDeclaredAsync(exchangeName, queueName, cancellationToken);

            return Result.Ok;
        }
        catch (Exception ex)
        {
            _recorder.TraceError(null, ex,
                "Failed to subscribe queue: {Queue} to exchange: {Exchange}",
                queueName, exchangeName);
            return ex.ToError(ErrorCode.Unexpected);
        }
    }

    private async Task PublishMessageAsync(string exchangeName, byte[] body, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            _channel.BasicPublish(
                exchange: exchangeName,
                routingKey: string.Empty,
                mandatory: false,
                basicProperties: null,
                body: body);
        }, cancellationToken);
    }

    private async Task EnsureExchangeDeclaredAsync(string exchangeName, CancellationToken cancellationToken)
    {
        await _syncLock.WaitAsync(cancellationToken);
        try
        {
            if (_declaredExchanges.ContainsKey(exchangeName)) return;

            await Task.Run(() =>
            {
                _channel.ExchangeDeclare(
                    exchange: exchangeName,
                    type: ExchangeType.Fanout,
                    durable: true,
                    autoDelete: false);
            }, cancellationToken);

            _declaredExchanges[exchangeName] = true;
        }
        finally
        {
            _syncLock.Release();
        }
    }

    private async Task EnsureQueueDeclaredAsync(string queueName, CancellationToken cancellationToken)
    {
        await _syncLock.WaitAsync(cancellationToken);
        try
        {
            await Task.Run(() =>
            {
                _channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
            }, cancellationToken);
        }
        finally
        {
            _syncLock.Release();
        }
    }

    private async Task EnsureBindingDeclaredAsync(string exchangeName, string queueName, CancellationToken cancellationToken)
    {
        var key = (exchangeName, queueName);

        await _syncLock.WaitAsync(cancellationToken);
        try
        {
            if (_declaredBindings.ContainsKey(key)) return;

            await Task.Run(() =>
            {
                _channel.QueueBind(
                    queue: queueName,
                    exchange: exchangeName,
                    routingKey: string.Empty);
            }, cancellationToken);

            _declaredBindings[key] = true;
        }
        finally
        {
            _syncLock.Release();
        }
    }
}
