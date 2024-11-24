using Application.Common.Extensions;
using Application.Interfaces;
using Application.Persistence.Shared.ReadModels;
using Common;
using Common.Extensions;

namespace AncillaryApplication;

partial class AncillaryApplication
{
    public async Task<Result<bool, Error>> DeliverUsageAsync(ICallerContext caller, string messageAsJson,
        CancellationToken cancellationToken)
    {
        var rehydrated = RehydrateMessage<UsageMessage>(messageAsJson);
        if (rehydrated.IsFailure)
        {
            return rehydrated.Error;
        }

        var delivered = await DeliverUsageInternalAsync(caller, rehydrated.Value, cancellationToken);
        if (delivered.IsFailure)
        {
            return delivered.Error;
        }

        _recorder.TraceInformation(caller.ToCall(), "Delivered usage message: {Message}", messageAsJson);
        return true;
    }

#if TESTINGONLY
    public async Task<Result<Error>> DrainAllUsagesAsync(ICallerContext caller, CancellationToken cancellationToken)
    {
        await DrainAllOnQueueAsync(_usageMessageQueue,
            message => DeliverUsageInternalAsync(caller, message, cancellationToken), cancellationToken);

        _recorder.TraceInformation(caller.ToCall(), "Drained all usage messages");

        return Result.Ok;
    }
#endif

    private async Task<Result<bool, Error>> DeliverUsageInternalAsync(ICallerContext caller, UsageMessage message,
        CancellationToken cancellationToken)
    {
        if (message.ForId.IsInvalidParameter(x => x.HasValue(), nameof(UsageMessage.ForId), out _))
        {
            return Error.RuleViolation(Resources.AncillaryApplication_Usage_MissingForId);
        }

        if (message.EventName.IsInvalidParameter(x => x.HasValue(), nameof(UsageMessage.EventName), out _))
        {
            return Error.RuleViolation(Resources.AncillaryApplication_Usage_MissingEventName);
        }

        var delivered = await _usageDeliveryService.DeliverAsync(caller, message.ForId!, message.EventName!,
            message.Additional,
            cancellationToken);
        if (delivered.IsFailure)
        {
            return delivered.Error;
        }

        _recorder.TraceInformation(caller.ToCall(), "Delivered usage for {For}", message.ForId!);

        return true;
    }
}