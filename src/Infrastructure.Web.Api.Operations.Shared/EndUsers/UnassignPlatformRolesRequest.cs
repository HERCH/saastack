using Infrastructure.Web.Api.Interfaces;

namespace Infrastructure.Web.Api.Operations.Shared.EndUsers;

[Route("/users/{id}/roles", ServiceOperation.PutPatch, AccessType.Token)]
[Authorize(Interfaces.Roles.Platform_Operations)]
public class UnassignPlatformRolesRequest : UnTenantedRequest<AssignPlatformRolesResponse>
{
    public required string Id { get; set; }

    public List<string>? Roles { get; set; }
}