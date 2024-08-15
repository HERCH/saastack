using Infrastructure.Web.Api.Interfaces;

namespace Infrastructure.Web.Api.Operations.Shared._3rdParties.Flagsmith;

/// <summary>
///     Deletes a feature
/// </summary>
[Route("/projects/{ProjectId}/features/{FeatureId}/", OperationMethod.Delete)]
public class FlagsmithDeleteFeatureRequest : WebRequestEmpty<FlagsmithDeleteFeatureRequest>
{
    public int? FeatureId { get; set; }

    public int? ProjectId { get; set; }
}