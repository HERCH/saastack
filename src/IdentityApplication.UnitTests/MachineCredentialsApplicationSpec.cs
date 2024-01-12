using Application.Interfaces;
using Application.Resources.Shared;
using Application.Services.Shared;
using Common;
using FluentAssertions;
using IdentityApplication.ApplicationServices;
using Moq;
using Xunit;

namespace IdentityApplication.UnitTests;

[Trait("Category", "Unit")]
public class MachineCredentialsApplicationSpec
{
    private readonly Mock<IAPIKeysService> _apiKeysService;
    private readonly MachineCredentialsApplication _application;
    private readonly Mock<ICallerContext> _caller;
    private readonly Mock<IEndUsersService> _endUsersService;

    public MachineCredentialsApplicationSpec()
    {
        var recorder = new Mock<IRecorder>();
        _caller = new Mock<ICallerContext>();
        _caller.Setup(cc => cc.CallerId)
            .Returns("acallerid");
        _endUsersService = new Mock<IEndUsersService>();
        _apiKeysService = new Mock<IAPIKeysService>();
        _application =
            new MachineCredentialsApplication(recorder.Object, _endUsersService.Object, _apiKeysService.Object);
    }

    [Fact]
    public async Task WhenRegisterMachine_ThenRegistersMachine()
    {
        var user = new RegisteredEndUser
        {
            Id = "amachineid"
        };
        _endUsersService.Setup(eus => eus.RegisterMachineAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Result<RegisteredEndUser, Error>>(user));
        var expiresOn = DateTime.UtcNow;
        var apiKey = new APIKey
        {
            Id = "anapikeyid",
            Key = "akey",
            UserId = "auserid",
            ExpiresOnUtc = expiresOn,
            Description = "adescription"
        };
        _apiKeysService.Setup(aks =>
                aks.CreateApiKeyAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Result<APIKey, Error>>(apiKey));

        var result = await _application.RegisterMachineAsync(_caller.Object, "aname", Timezones.Default.ToString(),
            CountryCodes.Default.ToString(), CancellationToken.None);

        result.Value.Id.Should().Be("amachineid");
        result.Value.ApiKey.Should().Be("akey");
        result.Value.Description.Should().Be("adescription");
        result.Value.CreatedById.Should().Be("acallerid");
        result.Value.ExpiresOnUtc.Should().Be(expiresOn);
    }
}