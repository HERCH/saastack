using Application.Resources.Shared;
using IdentityApplication;
using Infrastructure.Interfaces;
using Infrastructure.Web.Api.Common.Extensions;
using Infrastructure.Web.Api.Interfaces;
using Infrastructure.Web.Api.Operations.Shared.Identities;

namespace IdentityInfrastructure.Api.APIKeys;

public class APIKeysApi : IWebApiService
{
    private readonly IAPIKeysApplication _apiKeysApplication;
    private readonly ICallerContextFactory _contextFactory;

    public APIKeysApi(ICallerContextFactory contextFactory, IAPIKeysApplication apiKeysApplication)
    {
        _contextFactory = contextFactory;
        _apiKeysApplication = apiKeysApplication;
    }

#if TESTINGONLY
    public async Task<ApiPostResult<APIKey, CreateAPIKeyResponse>> RegisterMachine(
        CreateAPIKeyRequest request, CancellationToken cancellationToken)
    {
        var apiKey = await _apiKeysApplication.CreateAPIKeyAsync(_contextFactory.Create(), cancellationToken);

        return () => apiKey.HandleApplicationResult<CreateAPIKeyResponse, APIKey>(x =>
            new PostResult<CreateAPIKeyResponse>(new CreateAPIKeyResponse { ApiKey = x.Key }));
    }
#endif
}