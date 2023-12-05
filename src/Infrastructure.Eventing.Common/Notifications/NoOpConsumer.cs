using Common;
using Domain.Interfaces.Entities;
using Infrastructure.Eventing.Interfaces.Notifications;

namespace Infrastructure.Eventing.Common.Notifications;

/// <summary>
///     Provides a consumer that handles all events and does nothing with them
/// </summary>
public sealed class NoOpConsumer : IEventNotificationConsumer
{
    public Task<Result<bool, Error>> NotifyAsync(IDomainEvent changeEvent, CancellationToken cancellationToken)
    {
        return Task.FromResult<Result<bool, Error>>(true);
    }
}