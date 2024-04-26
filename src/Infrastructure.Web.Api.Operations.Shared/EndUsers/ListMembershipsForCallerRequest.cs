using Infrastructure.Web.Api.Interfaces;

namespace Infrastructure.Web.Api.Operations.Shared.EndUsers;

[Route("/memberships/me", OperationMethod.Search, AccessType.Token)]
[Authorize(Roles.Platform_Standard, Features.Platform_Basic)]
public class ListMembershipsForCallerRequest : UnTenantedSearchRequest<ListMembershipsForCallerResponse>
{
}