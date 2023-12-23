using Application.Persistence.Interfaces;
using Application.Persistence.Shared;
using Application.Services.Shared;
using Common;
using Infrastructure.Persistence.Common;
using Infrastructure.Persistence.Interfaces;

namespace AncillaryInfrastructure.Persistence;

public class AuditMessageQueueRepository : IAuditMessageQueueRepository
{
    private readonly MessageQueueStore<AuditMessage> _messageQueue;

    public AuditMessageQueueRepository(IRecorder recorder, IQueueStore store)
    {
        _messageQueue = new MessageQueueStore<AuditMessage>(recorder, store);
    }

    public Task<Result<long, Error>> CountAsync(CancellationToken cancellationToken)
    {
        return _messageQueue.CountAsync(cancellationToken);
    }

    public Task<Result<Error>> DestroyAllAsync(CancellationToken cancellationToken)
    {
        return _messageQueue.DestroyAllAsync(cancellationToken);
    }

    public Task<Result<bool, Error>> PopSingleAsync(
        Func<AuditMessage, CancellationToken, Task<Result<Error>>> onMessageReceivedAsync,
        CancellationToken cancellationToken)
    {
        return _messageQueue.PopSingleAsync(onMessageReceivedAsync, cancellationToken);
    }

    public Task<Result<Error>> PushAsync(ICallContext call, AuditMessage message, CancellationToken cancellationToken)
    {
        return _messageQueue.PushAsync(call, message, cancellationToken);
    }

    Task<Result<Error>> IApplicationRepository.DestroyAllAsync(CancellationToken cancellationToken)
    {
        return _messageQueue.DestroyAllAsync(cancellationToken);
    }
}