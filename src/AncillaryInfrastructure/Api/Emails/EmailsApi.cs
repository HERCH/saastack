using AncillaryApplication;
using Application.Resources.Shared;
using Common;
using Infrastructure.Interfaces;
using Infrastructure.Web.Api.Common.Extensions;
using Infrastructure.Web.Api.Interfaces;
using Infrastructure.Web.Api.Operations.Shared.Ancillary;

namespace AncillaryInfrastructure.Api.Emails;

public sealed class EmailsApi : IWebApiService
{
    private readonly IAncillaryApplication _ancillaryApplication;
    private readonly ICallerContextFactory _contextFactory;

    public EmailsApi(ICallerContextFactory contextFactory, IAncillaryApplication ancillaryApplication)
    {
        _contextFactory = contextFactory;
        _ancillaryApplication = ancillaryApplication;
    }

    public async Task<ApiPostResult<bool, DeliverMessageResponse>> Deliver(DeliverEmailRequest request,
        CancellationToken cancellationToken)
    {
        var delivered =
            await _ancillaryApplication.DeliverEmailAsync(_contextFactory.Create(), request.Message, cancellationToken);

        return () => delivered.HandleApplicationResult<DeliverMessageResponse, bool>(_ =>
            new PostResult<DeliverMessageResponse>(new DeliverMessageResponse { IsDelivered = true }));
    }

#if TESTINGONLY
    public async Task<ApiEmptyResult> DrainAll(DrainAllEmailsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _ancillaryApplication.DrainAllEmailsAsync(_contextFactory.Create(), cancellationToken);

        return () => result.Match(() => new Result<EmptyResponse, Error>(),
            error => new Result<EmptyResponse, Error>(error));
    }
#endif

    public async Task<ApiSearchResult<DeliveredEmail, SearchEmailDeliveriesResponse>> SearchAll(
        SearchEmailDeliveriesRequest request,
        CancellationToken cancellationToken)
    {
        var deliveries = await _ancillaryApplication.SearchAllEmailDeliveriesAsync(_contextFactory.Create(),
            request.SinceUtc,
            request.ToSearchOptions(),
            request.ToGetOptions(), cancellationToken);

        return () =>
            deliveries.HandleApplicationResult(c => new SearchEmailDeliveriesResponse
                { Emails = c.Results, Metadata = c.Metadata });
    }
}