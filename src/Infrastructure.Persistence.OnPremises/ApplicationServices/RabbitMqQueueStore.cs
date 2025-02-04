using Common;
using Common.Extensions;
using Infrastructure.Persistence.Interfaces;
using RabbitMQ.Client;
using System.Text;
using JetBrains.Annotations;

namespace Infrastructure.Persistence.OnPremises.ApplicationServices;

[UsedImplicitly]
public class RabbitMqQueueStore : IQueueStore, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IRecorder _recorder;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly Dictionary<string, bool> _declaredQueues = new();

    public static RabbitMqQueueStore Create(IRecorder recorder, RabbitMqStoreOptions options)
    {
        var factory = new ConnectionFactory
        {
            HostName = options.HostName,
            UserName = options.UserName,
            Password = options.Password,
            VirtualHost = options.VirtualHost ?? "/",
            DispatchConsumersAsync = true
        };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        return new RabbitMqQueueStore(recorder, connection, channel);
    }

    private RabbitMqQueueStore(IRecorder recorder, IConnection connection, IModel channel)
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

#if TESTINGONLY
    public async Task<Result<long, Error>> CountAsync(string queueName, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await DeclareQueueAsync(queueName);
            var result = _channel.QueueDeclarePassive(queueName);
            return result.MessageCount;
        }
        catch (Exception ex)
        {
            _recorder.TraceError(null, ex, "Failed to count messages in queue: {Queue}", queueName);
            return ex.ToError(ErrorCode.Unexpected);
        }
        finally
        {
            _lock.Release();
        }
    }
#endif

#if TESTINGONLY
    public async Task<Result<Error>> DestroyAllAsync(string queueName, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await DeclareQueueAsync(queueName);
            _channel.QueuePurge(queueName);
            return Result.Ok;
        }
        catch (Exception ex)
        {
            _recorder.TraceError(null, ex, "Failed to purge queue: {Queue}", queueName);
            return ex.ToError(ErrorCode.Unexpected);
        }
        finally
        {
            _lock.Release();
        }
    }
#endif

    public async Task<Result<bool, Error>> PopSingleAsync(
        string queueName,
        Func<string, CancellationToken, Task<Result<Error>>> messageHandlerAsync,
        CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await DeclareQueueAsync(queueName);

            var result = _channel.BasicGet(queueName, autoAck: false);
            if (result == null) return false;

            var body = Encoding.UTF8.GetString(result.Body.ToArray());

            try
            {
                var handled = await messageHandlerAsync(body, cancellationToken);
                if (handled.IsFailure)
                {
                    _channel.BasicNack(result.DeliveryTag, multiple: false, requeue: true);
                    return handled.Error;
                }

                _channel.BasicAck(result.DeliveryTag, multiple: false);
                return true;
            }
            catch (Exception ex)
            {
                _channel.BasicNack(result.DeliveryTag, multiple: false, requeue: true);
                _recorder.TraceError(null, ex, "Failed to process message from queue: {Queue}", queueName);
                return ex.ToError(ErrorCode.Unexpected);
            }
        }
        catch (Exception ex)
        {
            _recorder.TraceError(null, ex, "Failed to pop message from queue: {Queue}", queueName);
            return ex.ToError(ErrorCode.Unexpected);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Result<Error>> PushAsync(string queueName, string message, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await DeclareQueueAsync(queueName);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: "",
                routingKey: queueName,
                basicProperties: null,
                body: body);

            return Result.Ok;
        }
        catch (Exception ex)
        {
            _recorder.TraceError(null, ex, "Failed to push message to queue: {Queue}", queueName);
            return ex.ToError(ErrorCode.Unexpected);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task DeclareQueueAsync(string queueName)
    {
        if (_declaredQueues.ContainsKey(queueName)) return;

        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _declaredQueues[queueName] = true;
        await Task.CompletedTask;
    }
}