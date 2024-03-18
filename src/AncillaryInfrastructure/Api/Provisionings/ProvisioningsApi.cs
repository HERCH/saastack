using AncillaryApplication;
using Common;
using Infrastructure.Interfaces;
using Infrastructure.Web.Api.Common.Extensions;
using Infrastructure.Web.Api.Interfaces;
using Infrastructure.Web.Api.Operations.Shared.Ancillary;

namespace AncillaryInfrastructure.Api.Provisionings;

public sealed class ProvisioningsApi : IWebApiService
{
    private readonly IAncillaryApplication _ancillaryApplication;
    private readonly ICallerContextFactory _contextFactory;

    public ProvisioningsApi(ICallerContextFactory contextFactory, IAncillaryApplication ancillaryApplication)
    {
        _contextFactory = contextFactory;
        _ancillaryApplication = ancillaryApplication;
    }

    public async Task<ApiPostResult<bool, DeliverMessageResponse>> Notify(NotifyProvisioningRequest request,
        CancellationToken cancellationToken)
    {
        var delivered =
            await _ancillaryApplication.NotifyProvisioningAsync(_contextFactory.Create(), request.Message,
                cancellationToken);

        return () => delivered.HandleApplicationResult<DeliverMessageResponse, bool>(_ =>
            new PostResult<DeliverMessageResponse>(new DeliverMessageResponse { IsDelivered = true }));
    }

#if TESTINGONLY
    public async Task<ApiEmptyResult> DrainAll(DrainAllProvisioningsRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _ancillaryApplication.DrainAllProvisioningsAsync(_contextFactory.Create(), cancellationToken);

        return () => result.Match(() => new Result<EmptyResponse, Error>(),
            error => new Result<EmptyResponse, Error>(error));
    }
#endif
}