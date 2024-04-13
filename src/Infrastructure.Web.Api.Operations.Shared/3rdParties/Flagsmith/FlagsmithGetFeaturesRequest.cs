using Infrastructure.Web.Api.Interfaces;

namespace Infrastructure.Web.Api.Operations.Shared._3rdParties.Flagsmith;

[Route("/projects/{ProjectId}/features/", OperationMethod.Get)]
public class FlagsmithGetFeaturesRequest : IWebRequest<FlagsmithGetFeaturesResponse>
{
    public int ProjectId { get; set; }
}