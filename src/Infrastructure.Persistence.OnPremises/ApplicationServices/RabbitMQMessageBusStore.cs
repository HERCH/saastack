using System.Text;
using Common;
using Common.Extensions;
using Infrastructure.Persistence.Interfaces;
using Infrastructure.Persistence.OnPremises.Extensions;
using JetBrains.Annotations;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Infrastructure.Persistence.OnPremises.ApplicationServices;

[UsedImplicitly]
public sealed class RabbitMQMessageBusStore : IMessageBusStore, IAsyncDisposable
{
    private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(5);
    private readonly IConnection _connection;
    private readonly RabbitMqStoreOptions _connectionOptions;
    private readonly IRecorder _recorder;

    public static RabbitMQMessageBusStore Create(IRecorder recorder, RabbitMqStoreOptions options)
    {
        return new RabbitMQMessageBusStore(recorder, options);
    }

    private RabbitMQMessageBusStore(IRecorder recorder, RabbitMqStoreOptions connectionOptions)
    {
        _recorder = recorder;
        _connectionOptions = connectionOptions;
        _connection = CreateConnection();
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection.IsOpen)
        {
            await Task.Run(() => _connection.Close());
            _connection.Dispose();
        }
    }

#if TESTINGONLY
    public async Task<Result<long, Error>> CountAsync(string topicName, string subscriptionName,
        CancellationToken cancellationToken)
    {
        topicName.ThrowIfNotValuedParameter(nameof(topicName), Resources.AnyStore_MissingTopicName);
        subscriptionName.ThrowIfNotValuedParameter(nameof(subscriptionName), Resources.AnyStore_MissingSubscriptionName);

        var sanitizedTopicName = topicName.SanitizeAndValidateTopicName();
        var sanitizedSubscriptionName = subscriptionName.SanitizeAndValidateSubscriptionName();
        var queueName = GetQueueName(sanitizedTopicName, sanitizedSubscriptionName);

        try
        {
            using var channel = _connection.CreateModel();
            var queueDeclare = channel.QueueDeclarePassive(queueName);
            return queueDeclare.MessageCount;
        }
        catch (OperationInterruptedException ex) when (ex.ShutdownReason.ReplyCode == 404)
        {
            return 0;
        }
        catch (Exception ex)
        {
            return ex.ToError(ErrorCode.Unexpected);
        }
    }
#endif

#if TESTINGONLY
    public async Task<Result<Error>> DestroyAllAsync(string topicName, CancellationToken cancellationToken)
    {
        topicName.ThrowIfNotValuedParameter(nameof(topicName), Resources.AnyStore_MissingTopicName);

        var sanitizedTopicName = topicName.SanitizeAndValidateTopicName();

        try
        {
            using var channel = _connection.CreateModel();
            channel.ExchangeDelete(sanitizedTopicName);
            return Result.Ok;
        }
        catch (Exception ex)
        {
            return ex.ToError(ErrorCode.Unexpected);
        }
    }
#endif

#if TESTINGONLY
    public async Task<Result<bool, Error>> ReceiveSingleAsync(string topicName, string subscriptionName,
        Func<string, CancellationToken, Task<Result<Error>>> messageHandlerAsync,
        CancellationToken cancellationToken)
    {
        topicName.ThrowIfNotValuedParameter(nameof(topicName), Resources.AnyStore_MissingTopicName);
        subscriptionName.ThrowIfNotValuedParameter(nameof(subscriptionName), Resources.AnyStore_MissingSubscriptionName);
        ArgumentNullException.ThrowIfNull(messageHandlerAsync);

        var sanitizedTopicName = topicName.SanitizeAndValidateTopicName();
        var sanitizedSubscriptionName = subscriptionName.SanitizeAndValidateSubscriptionName();
        var queueName = GetQueueName(sanitizedTopicName, sanitizedSubscriptionName);

        using var channel = _connection.CreateModel();
        BasicGetResult result = null;
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < ReceiveTimeout)
        {
            result = channel.BasicGet(queueName, autoAck: false);
            if (result != null) break;
            await Task.Delay(100, cancellationToken);
        }

        if (result == null) return false;

        try
        {
            var message = Encoding.UTF8.GetString(result.Body.ToArray());
            var handled = await messageHandlerAsync(message, cancellationToken);

            if (handled.IsFailure)
            {
                channel.BasicNack(result.DeliveryTag, multiple: false, requeue: true);
                return handled.Error;
            }

            channel.BasicAck(result.DeliveryTag, multiple: false);
            return true;
        }
        catch (Exception ex)
        {
            channel.BasicNack(result.DeliveryTag, multiple: false, requeue: true);
            _recorder.TraceError(null, ex, "Error procesando mensaje: {Topic}, {Subscription}", topicName, subscriptionName);
            return ex.ToError(ErrorCode.Unexpected);
        }
    }
#endif

    public async Task<Result<Error>> SendAsync(string topicName, string message, CancellationToken cancellationToken)
    {
        topicName.ThrowIfNotValuedParameter(nameof(topicName), Resources.AnyStore_MissingTopicName);
        message.ThrowIfNotValuedParameter(nameof(message), Resources.AnyStore_MissingSentMessage);

        var sanitizedTopicName = topicName.SanitizeAndValidateTopicName();

        try
        {
            using var channel = _connection.CreateModel();
            channel.ExchangeDeclare(sanitizedTopicName, ExchangeType.Fanout, durable: true);
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(sanitizedTopicName, "", null, body);
            return Result.Ok;
        }
        catch (Exception ex)
        {
            _recorder.TraceError(null, ex, "Error enviando mensaje a {Topic}", topicName);
            return ex.ToError(ErrorCode.Unexpected);
        }
    }

    public async Task<Result<Error>> SubscribeAsync(string topicName, string subscriptionName,
        CancellationToken cancellationToken)
    {
        topicName.ThrowIfNotValuedParameter(nameof(topicName), Resources.AnyStore_MissingTopicName);
        subscriptionName.ThrowIfNotValuedParameter(nameof(subscriptionName), Resources.AnyStore_MissingSubscriptionName);

        var sanitizedTopicName = topicName.SanitizeAndValidateTopicName();
        var sanitizedSubscriptionName = subscriptionName.SanitizeAndValidateSubscriptionName();
        var queueName = GetQueueName(sanitizedTopicName, sanitizedSubscriptionName);

        try
        {
            using var channel = _connection.CreateModel();
            channel.ExchangeDeclare(sanitizedTopicName, ExchangeType.Fanout, durable: true);
            channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueBind(queueName, sanitizedTopicName, routingKey: "");
            return Result.Ok;
        }
        catch (Exception ex)
        {
            return ex.ToError(ErrorCode.Unexpected);
        }
    }

    private IConnection CreateConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = _connectionOptions.HostName,
            VirtualHost = _connectionOptions.VirtualHost ?? "/",
            UserName = _connectionOptions.UserName,
            Password = _connectionOptions.Password,
            DispatchConsumersAsync = true
        };
        return factory.CreateConnection();
    }

    private string GetQueueName(string topicName, string subscriptionName)
    {
        return $"{topicName}_{subscriptionName}";
    }
}