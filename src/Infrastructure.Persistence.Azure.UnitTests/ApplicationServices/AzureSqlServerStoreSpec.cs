using Common.Configuration;
using FluentAssertions;
using Infrastructure.Persistence.Azure.ApplicationServices;
using Moq;
using Xunit;

namespace Infrastructure.Persistence.Azure.UnitTests.ApplicationServices;

[Trait("Category", "Unit")]
public class AzureSqlServerStoreOptionsSpec
{
    private readonly Mock<IConfigurationSettings> _settings;

    public AzureSqlServerStoreOptionsSpec()
    {
        _settings = new Mock<IConfigurationSettings>();
    }

    [Fact]
    public void WhenCredentialsWithCredentials_ThenUsesConnectionString()
    {
        _settings.Setup(s => s.GetString(AzureSqlServerStoreOptions.DbCredentialsSettingName, It.IsAny<string>()))
            .Returns("acredentials");
        _settings.Setup(s => s.GetString(AzureSqlServerStoreOptions.DbNameSettingName, It.IsAny<string>()))
            .Returns("adbname");
        _settings.Setup(s => s.GetString(AzureSqlServerStoreOptions.DbServerNameSettingName, It.IsAny<string>()))
            .Returns("aservername");

        var result = AzureSqlServerStoreOptions.Credentials(_settings.Object);

        result.Connection.Type.Should().Be(AzureSqlServerStoreOptions.ConnectionOptions.ConnectionType.Credentials);
        result.Connection.ConnectionString.Should().Be("Persist Security Info=False;Encrypt=True;Initial Catalog=adbname;Server=aservername;acredentials");
        _settings.Verify(s => s.GetString(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
        _settings.Verify(s => s.GetString(AzureSqlServerStoreOptions.DbCredentialsSettingName, string.Empty));
        _settings.Verify(s => s.GetString(AzureSqlServerStoreOptions.DbNameSettingName, null));
        _settings.Verify(s => s.GetString(AzureSqlServerStoreOptions.DbServerNameSettingName, null));
    }
    [Fact]
    public void WhenCredentialsWithoutCredentials_ThenUsesConnectionString()
    {
        _settings.Setup(s => s.GetString(AzureSqlServerStoreOptions.DbCredentialsSettingName, It.IsAny<string>()))
            .Returns(string.Empty);
        _settings.Setup(s => s.GetString(AzureSqlServerStoreOptions.DbNameSettingName, It.IsAny<string>()))
            .Returns("adbname");
        _settings.Setup(s => s.GetString(AzureSqlServerStoreOptions.DbServerNameSettingName, It.IsAny<string>()))
            .Returns("aservername");

        var result = AzureSqlServerStoreOptions.Credentials(_settings.Object);

        result.Connection.Type.Should().Be(AzureSqlServerStoreOptions.ConnectionOptions.ConnectionType.Credentials);
        result.Connection.ConnectionString.Should().Be("Persist Security Info=False;Integrated Security=true;Encrypt=False;Initial Catalog=adbname;Server=aservername");
        _settings.Verify(s => s.GetString(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
        _settings.Verify(s => s.GetString(AzureSqlServerStoreOptions.DbCredentialsSettingName, string.Empty));
        _settings.Verify(s => s.GetString(AzureSqlServerStoreOptions.DbNameSettingName, null));
        _settings.Verify(s => s.GetString(AzureSqlServerStoreOptions.DbServerNameSettingName, null));
    }
    
    [Fact]
    public void WhenCustomConnectionString_ThenUsesConnectionString()
    {
        var result = AzureSqlServerStoreOptions.CustomConnectionString("aconnectionstring");

        result.Connection.Type.Should().Be(AzureSqlServerStoreOptions.ConnectionOptions.ConnectionType.Custom);
        result.Connection.ConnectionString.Should().Be("aconnectionstring");
        _settings.Verify(s => s.GetString(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void WhenUserManagedIdentity_ThenUsesCredentials()
    {
        _settings.Setup(s =>
                s.GetString(AzureSqlServerStoreOptions.ManagedIdentityClientIdSettingName, It.IsAny<string>()))
            .Returns("aclientid");
        _settings.Setup(s => s.GetString(AzureSqlServerStoreOptions.DbNameSettingName, It.IsAny<string>()))
            .Returns("adbname");
        _settings.Setup(s => s.GetString(AzureSqlServerStoreOptions.DbServerNameSettingName, It.IsAny<string>()))
            .Returns("aservername");

        var result = AzureSqlServerStoreOptions.UserManagedIdentity(_settings.Object);

        result.Connection.Type.Should()
            .Be(AzureSqlServerStoreOptions.ConnectionOptions.ConnectionType.ManagedIdentity);
        result.Connection.ConnectionString.Should().Be("Server=aservername;Authentication=Active Directory Managed Identity;Encrypt=True;User Id=aclientid;Database=adbname");
        _settings.Verify(s => s.GetString(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
        _settings.Verify(s =>
            s.GetString(AzureSqlServerStoreOptions.DbNameSettingName, null));
        _settings.Verify(s =>
            s.GetString(AzureSqlServerStoreOptions.DbServerNameSettingName, null));
        _settings.Verify(s =>
            s.GetString(AzureSqlServerStoreOptions.ManagedIdentityClientIdSettingName, null));
    }
}