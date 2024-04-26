using Infrastructure.Web.Api.Interfaces;

namespace Infrastructure.Web.Api.Operations.Shared.Organizations;

[Route("/organizations/{Id}", OperationMethod.PutPatch, AccessType.Token)]
[Authorize(Roles.Tenant_Owner, Features.Tenant_Basic)]
public class ChangeOrganizationRequest : UnTenantedRequest<GetOrganizationResponse>,
    IUnTenantedOrganizationRequest
{
    public string? Id { get; set; }

    public string? Name { get; set; }
}