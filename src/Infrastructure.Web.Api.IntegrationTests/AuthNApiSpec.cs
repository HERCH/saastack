#if TESTINGONLY
using System.Net;
using Application.Resources.Shared;
using Common.Configuration;
using Domain.Interfaces;
using Domain.Interfaces.Authorization;
using Domain.Services.Shared;
using FluentAssertions;
using IdentityInfrastructure.ApplicationServices;
using Infrastructure.Web.Api.Common.Extensions;
using Infrastructure.Web.Api.Operations.Shared.Identities;
using Infrastructure.Web.Api.Operations.Shared.TestingOnly;
using IntegrationTesting.WebApi.Common;
using Xunit;

namespace Infrastructure.Web.Api.IntegrationTests;

[Trait("Category", "Integration.API")]
[Collection("API")]
public class AuthNApiSpec : WebApiSpec<ApiHost1.Program>
{
    private readonly IConfigurationSettings _settings;
    private readonly ITokensService _tokensService;

    public AuthNApiSpec(WebApiSetup<ApiHost1.Program> setup) : base(setup)
    {
        EmptyAllRepositories();
        _settings = setup.GetRequiredService<IConfigurationSettings>();
        _tokensService = setup.GetRequiredService<ITokensService>();
    }

    [Fact]
    public async Task WhenGetHMACRequestWithNoHMACSignature_ThenReturns401()
    {
        var result = await Api.GetAsync(new GetCallerWithHMACTestingOnlyRequest());

        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.Content.Error.Title.Should().Be("Unauthorized");
    }

    [Fact]
    public async Task WhenGetHMACRequestWithWrongSignature_ThenReturns401()
    {
        var request = new GetCallerWithHMACTestingOnlyRequest();
        var result = await Api.GetAsync(request, req => req.SetHMACAuth(request, "awrongsecret"));

        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.Content.Error.Title.Should().Be("Unauthorized");
    }

    [Fact]
    public async Task WhenGetHMACRequestWithSignature_ThenReturnsSuccess()
    {
        var request = new GetCallerWithHMACTestingOnlyRequest();
        var result = await Api.GetAsync(request, req => req.SetHMACAuth(request, "asecret"));

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Content.Value.CallerId.Should().Be(CallerConstants.MaintenanceAccountUserId);
    }

    [Fact]
    public async Task WhenGetTokenRequestWithNoToken_ThenReturns401()
    {
        var result = await Api.GetAsync(new GetCallerWithTokenOrAPIKeyTestingOnlyRequest());

        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.Content.Error.Title.Should().Be("Unauthorized");
    }

    [Fact]
    public async Task WhenGetTokenRequestWithWrongToken_ThenReturns401()
    {
        var result = await Api.GetAsync(new GetCallerWithTokenOrAPIKeyTestingOnlyRequest(),
            req => req.SetJWTBearerToken("awrongtoken"));

        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.Content.Error.Title.Should().Be("Unauthorized");
    }

    [Fact]
    public async Task WhenGetTokenRequestWithToken_ThenReturnsSuccess()
    {
        var token = CreateJwtToken(_settings, _tokensService);

        var result = await Api.GetAsync(new GetCallerWithTokenOrAPIKeyTestingOnlyRequest(),
            req => req.SetJWTBearerToken(token));

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Content.Value.CallerId.Should().Be("auserid");
    }

    [Fact]
    public async Task WhenGetApiKeyRequestWithWrongAPIKey_ThenReturns401()
    {
        var result = await Api.GetAsync(new GetCallerWithTokenOrAPIKeyTestingOnlyRequest(),
            req => req.SetAPIKey("awrongapikey"));

        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.Content.Error.Title.Should().Be("Unauthorized");
    }

    [Fact]
    public async Task WhenGetApiKeyRequestWithAPIKey_ThenReturnsSuccess()
    {
        var login = await LoginUserAsync();
        var apiKey = await Api.PostAsync(new CreateAPIKeyForCallerRequest(),
            req => req.SetJWTBearerToken(login.AccessToken));

        var result = await Api.GetAsync(new GetCallerWithTokenOrAPIKeyTestingOnlyRequest(),
            req => req.SetAPIKey(apiKey.Content.Value.ApiKey!));

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Content.Value.CallerId.Should().Be(login.User.Id);
    }

    private static string CreateJwtToken(IConfigurationSettings settings, ITokensService tokensService)
    {
        return new JWTTokensService(settings, tokensService)
            .IssueTokensAsync(new EndUserWithMemberships
            {
                Id = "auserid",
                Roles = new List<string> { PlatformRoles.Standard.Name },
                Features = new List<string> { PlatformFeatures.Basic.Name }
            }).GetAwaiter().GetResult().Value.AccessToken;
    }
}
#endif