using Application.Interfaces;
using Application.Interfaces.Services;
using Application.Resources.Shared;
using Common;

namespace OrganizationsApplication;

public interface IOrganizationsApplication
{
    Task<Result<Error>> ChangeSettingsAsync(ICallerContext caller, string id,
        TenantSettings settings, CancellationToken cancellationToken);

    Task<Result<Organization, Error>> CreateOrganizationAsync(ICallerContext caller, string creatorId, string name,
        OrganizationOwnership ownership, CancellationToken cancellationToken);

    Task<Result<Organization, Error>> CreateSharedOrganizationAsync(ICallerContext caller, string name,
        CancellationToken cancellationToken);

    Task<Result<Organization, Error>> GetOrganizationAsync(ICallerContext caller, string id,
        CancellationToken cancellationToken);

#if TESTINGONLY
    Task<Result<OrganizationWithSettings, Error>> GetOrganizationSettingsAsync(ICallerContext caller, string id,
        CancellationToken cancellationToken);
#endif

    Task<Result<TenantSettings, Error>> GetSettingsAsync(ICallerContext caller, string id,
        CancellationToken cancellationToken);
}