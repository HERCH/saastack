using Common;

namespace Application.Persistence.Interfaces;

/// <summary>
///     Defines a store for reading and writing a message on a queue
/// </summary>
public interface IMessageQueueStore<TMessage>
    where TMessage : IQueuedMessage, new()
{
    /// <summary>
    ///     Returns the total count of messages in the queue
    /// </summary>
    Task<Result<long, Error>> CountAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Permanently destroys all messages in the store
    /// </summary>
    Task<Result<Error>> DestroyAllAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Fetches the first message on the top of the queue and executes the <see cref="onMessageReceivedAsync" /> handler
    ///     with
    ///     it.
    /// </summary>
    Task<Result<bool, Error>> PopSingleAsync(
        Func<TMessage, CancellationToken, Task<Result<Error>>> onMessageReceivedAsync,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Adds a new message to the queue
    /// </summary>
    Task<Result<Error>> PushAsync(ICallContext call, TMessage message, CancellationToken cancellationToken);
}