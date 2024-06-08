using Application.Interfaces;
using Application.Resources.Shared;
using Common;
using Audit = Application.Resources.Shared.Audit;

namespace AncillaryApplication;

public interface IAncillaryApplication
{
    Task<Result<bool, Error>> DeliverAuditAsync(ICallerContext caller, string messageAsJson,
        CancellationToken cancellationToken);

    Task<Result<bool, Error>> DeliverEmailAsync(ICallerContext caller, string messageAsJson,
        CancellationToken cancellationToken);

    Task<Result<bool, Error>> DeliverUsageAsync(ICallerContext caller, string messageAsJson,
        CancellationToken cancellationToken);

#if TESTINGONLY
    Task<Result<Error>> DrainAllAuditsAsync(ICallerContext caller, CancellationToken cancellationToken);
#endif

#if TESTINGONLY
    Task<Result<Error>> DrainAllEmailsAsync(ICallerContext caller, CancellationToken cancellationToken);
#endif

#if TESTINGONLY
    Task<Result<Error>> DrainAllProvisioningsAsync(ICallerContext caller, CancellationToken cancellationToken);
#endif

#if TESTINGONLY
    Task<Result<Error>> DrainAllUsagesAsync(ICallerContext caller, CancellationToken cancellationToken);
#endif

    Task<Result<bool, Error>> NotifyProvisioningAsync(ICallerContext caller, string messageAsJson,
        CancellationToken cancellationToken);

#if TESTINGONLY
    Task<Result<SearchResults<Audit>, Error>> SearchAllAuditsAsync(ICallerContext caller, string organizationId,
        SearchOptions searchOptions, GetOptions getOptions, CancellationToken cancellationToken);
#endif

    Task<Result<SearchResults<DeliveredEmail>, Error>> SearchAllEmailDeliveriesAsync(ICallerContext caller,
        DateTime? sinceUtc, SearchOptions searchOptions, GetOptions getOptions, CancellationToken cancellationToken);
}