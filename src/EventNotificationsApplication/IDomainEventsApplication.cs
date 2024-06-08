using Application.Interfaces;
using Application.Resources.Shared;
using Common;

namespace EventNotificationsApplication;

public interface IDomainEventsApplication
{
#if TESTINGONLY
    Task<Result<Error>> DrainAllDomainEventsAsync(ICallerContext caller, CancellationToken cancellationToken);
#endif

    Task<Result<bool, Error>> NotifyDomainEventAsync(ICallerContext caller, string messageAsJson,
        CancellationToken cancellationToken);

#if TESTINGONLY
    Task<Result<SearchResults<DomainEvent>, Error>> SearchAllDomainEventsAsync(ICallerContext caller,
        SearchOptions searchOptions, GetOptions getOptions, CancellationToken cancellationToken);
#endif
}