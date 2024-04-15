using Infrastructure.Web.Api.Interfaces;

namespace Infrastructure.Web.Api.Operations.Shared.Organizations;

[Route("/organizations/{Id}", OperationMethod.Get, AccessType.Token)]
[Authorize(Roles.Tenant_Member, Features.Tenant_Basic)]
public class GetOrganizationRequest : UnTenantedRequest<GetOrganizationResponse>, IUnTenantedOrganizationRequest
{
    public string? Id { get; set; }
}