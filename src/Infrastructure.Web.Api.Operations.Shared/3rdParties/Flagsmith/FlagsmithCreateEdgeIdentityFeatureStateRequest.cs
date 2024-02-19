using System.Text.Json.Serialization;
using Infrastructure.Web.Api.Interfaces;

namespace Infrastructure.Web.Api.Operations.Shared._3rdParties.Flagsmith;

[Route("/environments/{EnvironmentApiKey}/edge-identities/{IdentityUuid}/edge-featurestates/", ServiceOperation.Post)]
public class
    FlagsmithCreateEdgeIdentityFeatureStateRequest : IWebRequest<FlagsmithCreateEdgeIdentityFeatureStateResponse>
{
    [JsonPropertyName("enabled")] public bool Enabled { get; set; }

    public required string EnvironmentApiKey { get; set; }

    [JsonPropertyName("feature")] public int Feature { get; set; }

    public required string IdentityUuid { get; set; }
}

public class FlagsmithCreateEdgeIdentityFeatureStateResponse : IWebResponse
{
}