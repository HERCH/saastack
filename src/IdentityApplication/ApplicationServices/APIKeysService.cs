using Application.Interfaces;
using Application.Resources.Shared;
using Common;

namespace IdentityApplication.ApplicationServices;

public class APIKeysService : IAPIKeysService
{
    private readonly IAPIKeysApplication _apiKeysApplication;

    public APIKeysService(IAPIKeysApplication apiKeysApplication)
    {
        _apiKeysApplication = apiKeysApplication;
    }

    public async Task<Result<APIKey, Error>> CreateApiKeyAsync(ICallerContext caller, string userId,
        string description, DateTime? expiresOn, CancellationToken cancellationToken)
    {
        return await _apiKeysApplication.CreateAPIKeyAsync(caller, userId, description, expiresOn, cancellationToken);
    }
}