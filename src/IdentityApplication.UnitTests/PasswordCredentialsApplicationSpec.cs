using Application.Interfaces;
using Application.Resources.Shared;
using Application.Services.Shared;
using Common;
using Common.Configuration;
using Common.Extensions;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Services.Shared;
using Domain.Shared;
using FluentAssertions;
using IdentityApplication.ApplicationServices;
using IdentityApplication.Persistence;
using IdentityDomain;
using IdentityDomain.DomainServices;
using Moq;
using UnitTesting.Common;
using Xunit;
using PersonName = Application.Resources.Shared.PersonName;
using Task = System.Threading.Tasks.Task;

namespace IdentityApplication.UnitTests;

[Trait("Category", "Unit")]
public class PasswordCredentialsApplicationSpec
{
    private readonly PasswordCredentialsApplication _application;
    private readonly Mock<IAuthTokensService> _authTokensService;
    private readonly Mock<ICallerContext> _caller;
    private readonly Mock<IEmailAddressService> _emailAddressService;
    private readonly Mock<IEncryptionService> _encryptionService;
    private readonly Mock<IEndUsersService> _endUsersService;
    private readonly Mock<IIdentifierFactory> _idFactory;
    private readonly Mock<IMfaService> _mfaService;
    private readonly Mock<IUserNotificationsService> _notificationsService;
    private readonly Mock<IPasswordHasherService> _passwordHasherService;
    private readonly Mock<IRecorder> _recorder;
    private readonly Mock<IPasswordCredentialsRepository> _repository;
    private readonly Mock<IConfigurationSettings> _settings;
    private readonly Mock<ITokensService> _tokensService;
    private readonly Mock<IUserProfilesService> _userProfilesService;

    public PasswordCredentialsApplicationSpec()
    {
        _recorder = new Mock<IRecorder>();
        _idFactory = new Mock<IIdentifierFactory>();
        _idFactory.Setup(idf => idf.Create(It.IsAny<IIdentifiableEntity>()))
            .Returns("anid".ToId());
        _caller = new Mock<ICallerContext>();
        _caller.Setup(cc => cc.CallerId)
            .Returns("acallerid");
        _endUsersService = new Mock<IEndUsersService>();
        _userProfilesService = new Mock<IUserProfilesService>();
        _notificationsService = new Mock<IUserNotificationsService>();
        _settings = new Mock<IConfigurationSettings>();
        _settings.Setup(s => s.Platform.GetString(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string?)null!);
        _settings.Setup(s => s.Platform.GetNumber(It.IsAny<string>(), It.IsAny<double>()))
            .Returns(5);
        _emailAddressService = new Mock<IEmailAddressService>();
        _emailAddressService.Setup(eas => eas.EnsureUniqueAsync(It.IsAny<EmailAddress>(), It.IsAny<Identifier>()))
            .ReturnsAsync(true);
        _tokensService = new Mock<ITokensService>();
        _tokensService.Setup(ts => ts.CreateRegistrationVerificationToken())
            .Returns("averificationtoken");
        _tokensService.Setup(ts => ts.CreateMfaAuthenticationToken())
            .Returns("anmfatoken");
        _encryptionService = new Mock<IEncryptionService>();
        _passwordHasherService = new Mock<IPasswordHasherService>();
        _passwordHasherService.Setup(phs => phs.ValidatePassword(It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(true);
        _passwordHasherService.Setup(phs => phs.HashPassword(It.IsAny<string>()))
            .Returns("apasswordhash");
        _passwordHasherService.Setup(phs => phs.ValidatePasswordHash(It.IsAny<string>()))
            .Returns(true);
        _passwordHasherService.Setup(phs => phs.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        _mfaService = new Mock<IMfaService>();
        _authTokensService = new Mock<IAuthTokensService>();
        var websiteUiService = new Mock<IWebsiteUiService>();
        _repository = new Mock<IPasswordCredentialsRepository>();
        _repository.Setup(rep => rep.SaveAsync(It.IsAny<PasswordCredentialRoot>(), It.IsAny<CancellationToken>()))
            .Returns((PasswordCredentialRoot root, CancellationToken _) =>
                Task.FromResult<Result<PasswordCredentialRoot, Error>>(root));
        _repository.Setup(rep =>
                rep.SaveAsync(It.IsAny<PasswordCredentialRoot>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns((PasswordCredentialRoot root, bool _, CancellationToken _) =>
                Task.FromResult<Result<PasswordCredentialRoot, Error>>(root));

        _application = new PasswordCredentialsApplication(_recorder.Object, _idFactory.Object, _endUsersService.Object,
            _userProfilesService.Object, _notificationsService.Object, _settings.Object, _emailAddressService.Object,
            _tokensService.Object, _encryptionService.Object, _passwordHasherService.Object, _mfaService.Object,
            _authTokensService.Object,
            websiteUiService.Object,
            _repository.Object);
    }

    [Fact]
    public async Task WhenAuthenticateAsyncAndNoCredentials_ThenReturnsError()
    {
        _repository.Setup(rep => rep.FindCredentialsByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Optional<PasswordCredentialRoot>
                .None);

        var result =
            await _application.AuthenticateAsync(_caller.Object, "ausername", "apassword", CancellationToken.None);

        result.Should().BeError(ErrorCode.NotAuthenticated);
    }

    [Fact]
    public async Task WhenAuthenticateAsyncAndUnknownUser_ThenReturnsError()
    {
        var credential = CreateCredential();
        _repository.Setup(rep => rep.FindCredentialsByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(credential.ToOptional());
        _endUsersService.Setup(eus =>
                eus.GetMembershipsPrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.EntityNotFound());

        var result =
            await _application.AuthenticateAsync(_caller.Object, "ausername", "apassword", CancellationToken.None);

        result.Should().BeError(ErrorCode.NotAuthenticated);
        _repository.Verify(rep => rep.SaveAsync(It.IsAny<PasswordCredentialRoot>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _recorder.Verify(rec => rec.AuditAgainst(It.IsAny<ICallContext>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public async Task WhenAuthenticateAsyncAndEndUserIsNotRegistered_ThenReturnsError()
    {
        var credential = CreateCredential();
        _repository.Setup(rep => rep.FindCredentialsByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(credential.ToOptional());
        _endUsersService.Setup(eus =>
                eus.GetMembershipsPrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EndUserWithMemberships
            {
                Id = "anid",
                Status = EndUserStatus.Unregistered
            });

        var result =
            await _application.AuthenticateAsync(_caller.Object, "ausername", "apassword", CancellationToken.None);

        result.Should().BeError(ErrorCode.NotAuthenticated);
        _repository.Verify(rep => rep.SaveAsync(It.IsAny<PasswordCredentialRoot>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _recorder.Verify(rec => rec.AuditAgainst(It.IsAny<ICallContext>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public async Task WhenAuthenticateAsyncAndEndUserIsNotPerson_ThenReturnsError()
    {
        var credential = CreateCredential();
        _repository.Setup(rep => rep.FindCredentialsByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(credential.ToOptional());
        _endUsersService.Setup(eus =>
                eus.GetMembershipsPrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EndUserWithMemberships
            {
                Id = "auserid",
                Status = EndUserStatus.Registered,
                Classification = EndUserClassification.Machine
            });

        var result =
            await _application.AuthenticateAsync(_caller.Object, "ausername", "apassword", CancellationToken.None);

        result.Should().BeError(ErrorCode.NotAuthenticated);
        _repository.Verify(rep => rep.SaveAsync(It.IsAny<PasswordCredentialRoot>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _recorder.Verify(rec => rec.AuditAgainst(It.IsAny<ICallContext>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public async Task WhenAuthenticateAsyncAndEndUserIsSuspended_ThenReturnsError()
    {
        var credential = CreateCredential();
        _repository.Setup(rep => rep.FindCredentialsByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(credential.ToOptional());
        _endUsersService.Setup(eus =>
                eus.GetMembershipsPrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EndUserWithMemberships
            {
                Id = "auserid",
                Status = EndUserStatus.Registered,
                Classification = EndUserClassification.Person,
                Access = EndUserAccess.Suspended
            });

        var result =
            await _application.AuthenticateAsync(_caller.Object, "ausername", "apassword", CancellationToken.None);

        result.Should().BeError(ErrorCode.EntityLocked, Resources.PasswordCredentialsApplication_AccountSuspended);
        _repository.Verify(rep => rep.SaveAsync(It.IsAny<PasswordCredentialRoot>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _recorder.Verify(rec => rec.AuditAgainst(It.IsAny<ICallContext>(), "auserid",
            Audits.PasswordCredentialsApplication_Authenticate_AccountSuspended, It.IsAny<string>(),
            It.IsAny<object[]>()));
    }

    [Fact]
    public async Task WhenAuthenticateAsyncAndCredentialsIsLocked_ThenReturnsError()
    {
        var credential = CreateVerifiedCredential();
        _passwordHasherService.Setup(phs => phs.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);
#if TESTINGONLY
        credential.TestingOnly_LockAccount("awrongpassword");
#endif
        _repository.Setup(rep => rep.FindCredentialsByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(credential.ToOptional());
        _endUsersService.Setup(eus =>
                eus.GetMembershipsPrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EndUserWithMemberships
            {
                Id = "auserid",
                Status = EndUserStatus.Registered,
                Classification = EndUserClassification.Person,
                Access = EndUserAccess.Enabled
            });

        var result =
            await _application.AuthenticateAsync(_caller.Object, "ausername", "apassword", CancellationToken.None);

        result.Should().BeError(ErrorCode.EntityLocked, Resources.PasswordCredentialsApplication_AccountLocked);
        _repository.Verify(rep => rep.SaveAsync(It.IsAny<PasswordCredentialRoot>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _recorder.Verify(rec => rec.AuditAgainst(It.IsAny<ICallContext>(), "auserid",
            Audits.PasswordCredentialsApplication_Authenticate_AccountLocked, It.IsAny<string>(),
            It.IsAny<object[]>()));
    }

    [Fact]
    public async Task WhenAuthenticateAsyncAndWrongPassword_ThenReturnsError()
    {
        var credential = CreateUnVerifiedCredential();
        _repository.Setup(rep => rep.FindCredentialsByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(credential.ToOptional());
        _endUsersService.Setup(eus =>
                eus.GetMembershipsPrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EndUserWithMemberships
            {
                Id = "auserid",
                Status = EndUserStatus.Registered,
                Classification = EndUserClassification.Person,
                Access = EndUserAccess.Enabled
            });
        _passwordHasherService.Setup(phs => phs.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        var result =
            await _application.AuthenticateAsync(_caller.Object, "ausername", "awrongpassword", CancellationToken.None);

        result.Should().BeError(ErrorCode.NotAuthenticated);
        _repository.Verify(rep =>
            rep.SaveAsync(It.IsAny<PasswordCredentialRoot>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()));
        _recorder.Verify(rec => rec.AuditAgainst(It.IsAny<ICallContext>(), "auserid",
            Audits.PasswordCredentialsApplication_Authenticate_InvalidCredentials, It.IsAny<string>(),
            It.IsAny<object[]>()));
    }

    [Fact]
    public async Task WhenAuthenticateAsyncAndCredentialsNotYetVerified_ThenReturnsError()
    {
        var credential = CreateUnVerifiedCredential();
        _repository.Setup(rep => rep.FindCredentialsByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(credential.ToOptional());
        _endUsersService.Setup(eus =>
                eus.GetMembershipsPrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EndUserWithMemberships
            {
                Id = "auserid",
                Status = EndUserStatus.Registered,
                Classification = EndUserClassification.Person,
                Access = EndUserAccess.Enabled
            });

        var result =
            await _application.AuthenticateAsync(_caller.Object, "ausername", "apassword", CancellationToken.None);

        result.Should().BeError(ErrorCode.PreconditionViolation,
            Resources.PasswordCredentialsApplication_RegistrationNotVerified);
        _repository.Verify(rep =>
            rep.SaveAsync(It.IsAny<PasswordCredentialRoot>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()));
        _recorder.Verify(rec => rec.AuditAgainst(It.IsAny<ICallContext>(), "auserid",
            Audits.PasswordCredentialsApplication_Authenticate_BeforeVerified, It.IsAny<string>(),
            It.IsAny<object[]>()));
    }

    [Fact]
    public async Task WhenAuthenticateAsyncWithCorrectPasswordAndMfa_ThenReturnsError()
    {
        _caller.Setup(cc => cc.CallId)
            .Returns("acallid");
        var credential = CreateVerifiedCredential();
        credential.ChangeMfaEnabled("auserid".ToId(), true);
        _repository.Setup(rep => rep.FindCredentialsByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(credential.ToOptional());
        var user = new EndUserWithMemberships
        {
            Id = "auserid",
            Status = EndUserStatus.Registered,
            Classification = EndUserClassification.Person,
            Access = EndUserAccess.Enabled,
            Memberships =
            [
                new Membership
                {
                    Id = "amembershipid",
                    IsDefault = true,
                    OrganizationId = "anorganizationid",
                    UserId = "auserid"
                }
            ]
        };
        _endUsersService.Setup(eus =>
                eus.GetMembershipsPrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userProfilesService.Setup(ups =>
                ups.GetProfilePrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfile
            {
                Id = "aprofileid",
                UserId = "auserid",
                Name = new PersonName
                {
                    FirstName = "afirstname",
                    LastName = "alastname"
                },
                DisplayName = "adisplayname"
            });
        var expiresOn = DateTime.UtcNow;
        _authTokensService.Setup(jts =>
                jts.IssueTokensAsync(It.IsAny<ICallerContext>(), It.IsAny<EndUserWithMemberships>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessTokens("anaccesstoken", expiresOn,
                "arefreshtoken", expiresOn));

        var result =
            await _application.AuthenticateAsync(_caller.Object, "ausername", "apassword", CancellationToken.None);

        result.Should().BeError(ErrorCode.ForbiddenAccess, Resources.PasswordCredentialsApplication_MfaRequired,
            error => error.AdditionalCode == PasswordCredentialsApplication.MfaRequiredCode
                     && error.AdditionalData!.Count == 1
                     && (string)error.AdditionalData[PasswordCredentialsApplication.MfaTokenName] == "anmfatoken");
        _userProfilesService.Verify(ups =>
            ups.GetProfilePrivateAsync(It.Is<ICallerContext>(cc =>
                cc.CallId == "acallid"
            ), "auserid", It.IsAny<CancellationToken>()));
        _repository.Verify(rep => rep.SaveAsync(It.IsAny<PasswordCredentialRoot>(), It.IsAny<CancellationToken>()));
        _recorder.Verify(rec => rec.AuditAgainst(It.IsAny<ICallContext>(), "auserid",
            Audits.PasswordCredentialsApplication_Authenticate_Succeeded, It.IsAny<string>(),
            It.IsAny<object[]>()));
        _tokensService.Verify(ts => ts.CreateMfaAuthenticationToken());
        _authTokensService.Verify(jts =>
            jts.IssueTokensAsync(It.IsAny<ICallerContext>(), It.IsAny<EndUserWithMemberships>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenAuthenticateAsyncWithCorrectPasswordAndNotMfa_ThenAuthenticates()
    {
        _caller.Setup(cc => cc.CallId)
            .Returns("acallid");
        var credential = CreateVerifiedCredential();
        _repository.Setup(rep => rep.FindCredentialsByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(credential.ToOptional());
        var user = new EndUserWithMemberships
        {
            Id = "auserid",
            Status = EndUserStatus.Registered,
            Classification = EndUserClassification.Person,
            Access = EndUserAccess.Enabled,
            Memberships =
            [
                new Membership
                {
                    Id = "amembershipid",
                    IsDefault = true,
                    OrganizationId = "anorganizationid",
                    UserId = "auserid"
                }
            ]
        };
        _endUsersService.Setup(eus =>
                eus.GetMembershipsPrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userProfilesService.Setup(ups =>
                ups.GetProfilePrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfile
            {
                Id = "aprofileid",
                UserId = "auserid",
                Name = new PersonName
                {
                    FirstName = "afirstname",
                    LastName = "alastname"
                },
                DisplayName = "adisplayname"
            });
        var expiresOn = DateTime.UtcNow;
        _authTokensService.Setup(jts =>
                jts.IssueTokensAsync(It.IsAny<ICallerContext>(), It.IsAny<EndUserWithMemberships>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessTokens("anaccesstoken", expiresOn,
                "arefreshtoken", expiresOn));

        var result =
            await _application.AuthenticateAsync(_caller.Object, "ausername", "apassword", CancellationToken.None);

        result.Should().BeSuccess();
        result.Value.AccessToken.Value.Should().Be("anaccesstoken");
        result.Value.RefreshToken.Value.Should().Be("arefreshtoken");
        result.Value.AccessToken.ExpiresOn.Should().Be(expiresOn);
        result.Value.RefreshToken.ExpiresOn.Should().Be(expiresOn);
        _userProfilesService.Verify(ups =>
            ups.GetProfilePrivateAsync(It.Is<ICallerContext>(cc =>
                cc.CallId == "acallid"
            ), "auserid", It.IsAny<CancellationToken>()));
        _repository.Verify(rep =>
            rep.SaveAsync(It.IsAny<PasswordCredentialRoot>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()));
        _recorder.Verify(rec => rec.AuditAgainst(It.IsAny<ICallContext>(), "auserid",
            Audits.PasswordCredentialsApplication_Authenticate_Succeeded, It.IsAny<string>(),
            It.IsAny<object[]>()));
        _authTokensService.Verify(jts =>
            jts.IssueTokensAsync(It.IsAny<ICallerContext>(), user, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task WhenAuthenticateAsyncAndSendingEmailFails_ThenReturnsError()
    {
        var registeredAccount = new EndUser
        {
            Id = "auserid"
        };
        _endUsersService.Setup(uas => uas.RegisterPersonPrivateAsync(It.IsAny<ICallerContext>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(registeredAccount);
        _repository.Setup(s => s.FindCredentialsByUserIdAsync(It.IsAny<Identifier>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Optional<PasswordCredentialRoot>
                .None);
        _notificationsService.Setup(ns =>
                ns.NotifyPasswordRegistrationConfirmationAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Unexpected());

        var result = await _application.RegisterPersonAsync(_caller.Object, "aninvitationtoken", "afirstname",
            "alastname", "auser@company.com", "apassword", "atimezone", "acountrycode", true, CancellationToken.None);

        result.Should().BeError(ErrorCode.Unexpected);
        _repository.Verify(s => s.SaveAsync(It.IsAny<PasswordCredentialRoot>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _notificationsService.Verify(ns =>
            ns.NotifyPasswordRegistrationConfirmationAsync(_caller.Object, "auser@company.com", "afirstname",
                "averificationtoken", It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()));
        _endUsersService.Verify(eus => eus.RegisterPersonPrivateAsync(_caller.Object, "aninvitationtoken",
            "auser@company.com", "afirstname", "alastname", "atimezone", "acountrycode", true,
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task WhenRegisterPersonAsyncAndAlreadyExists_ThenDoesNothing()
    {
        var endUser = new EndUser
        {
            Id = "auserid"
        };
        _endUsersService.Setup(uas => uas.RegisterPersonPrivateAsync(It.IsAny<ICallerContext>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(endUser);
        var credential = CreateUnVerifiedCredential();
        _repository.Setup(s => s.FindCredentialsByUserIdAsync(It.IsAny<Identifier>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(credential.ToOptional());

        var result = await _application.RegisterPersonAsync(_caller.Object, "aninvitationtoken", "afirstname",
            "alastname", "auser@company.com", "apassword", "atimezone", "acountrycode", true, CancellationToken.None);

        result.Value.User.Should().Be(endUser);
        _repository.Verify(s => s.SaveAsync(It.IsAny<PasswordCredentialRoot>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _endUsersService.Verify(uas => uas.RegisterPersonPrivateAsync(_caller.Object, "aninvitationtoken",
            "auser@company.com", "afirstname", "alastname", "atimezone", "acountrycode", true,
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task WhenRegisterPersonAsyncAndNotExists_ThenCreatesAndSendsConfirmation()
    {
        var registeredAccount = new EndUser
        {
            Id = "auserid"
        };
        _endUsersService.Setup(uas => uas.RegisterPersonPrivateAsync(It.IsAny<ICallerContext>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(registeredAccount);
        _repository.Setup(s => s.FindCredentialsByUserIdAsync(It.IsAny<Identifier>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Optional<PasswordCredentialRoot>
                .None);

        var result = await _application.RegisterPersonAsync(_caller.Object, "aninvitationtoken", "afirstname",
            "alastname", "auser@company.com", "apassword", "atimezone", "acountrycode", true, CancellationToken.None);

        result.Value.User.Should().Be(registeredAccount);
        _repository.Verify(s => s.SaveAsync(It.Is<PasswordCredentialRoot>(uc =>
            uc.Id == "anid"
            && uc.UserId == "auserid"
            && uc.Registration.Value.Name == "afirstname"
            && uc.Registration.Value.EmailAddress == "auser@company.com"
            && uc.Password.PasswordHash == "apasswordhash"
            && uc.Login.Exists()
            && !uc.VerificationKeep.IsVerified
        ), It.IsAny<CancellationToken>()));
        _notificationsService.Verify(ns =>
            ns.NotifyPasswordRegistrationConfirmationAsync(_caller.Object, "auser@company.com", "afirstname",
                "averificationtoken", UserNotificationConstants.EmailTags.RegisterPerson,
                It.IsAny<CancellationToken>()));
        _endUsersService.Verify(eus => eus.RegisterPersonPrivateAsync(_caller.Object, "aninvitationtoken",
            "auser@company.com", "afirstname", "alastname", "atimezone", "acountrycode", true,
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task WhenConfirmPersonRegistrationAsyncAndTokenUnknown_ThenReturnsError()
    {
        _repository.Setup(s =>
                s.FindCredentialsByRegistrationVerificationTokenAsync(It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Optional<PasswordCredentialRoot>
                .None);

        var result =
            await _application.ConfirmPersonRegistrationAsync(_caller.Object, "atoken", CancellationToken.None);

        result.Should().BeError(ErrorCode.EntityNotFound);
        _repository.Verify(s => s.SaveAsync(It.IsAny<PasswordCredentialRoot>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task WhenConfirmPersonRegistrationAsync_ThenReturnsSuccess()
    {
        var credential = CreateUnVerifiedCredential();
        _repository.Setup(s =>
                s.FindCredentialsByRegistrationVerificationTokenAsync(It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(credential.ToOptional());

        var result =
            await _application.ConfirmPersonRegistrationAsync(_caller.Object, "atoken", CancellationToken.None);

        result.Should().BeSuccess();
        _repository.Verify(s => s.SaveAsync(It.Is<PasswordCredentialRoot>(pc =>
            pc.IsVerified
            && pc.IsRegistrationVerified
        ), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task WhenGetPasswordCredentialAsyncAndNotFound_ThenReturnsError()
    {
        _repository.Setup(s => s.FindCredentialsByUserIdAsync(It.IsAny<Identifier>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Optional<PasswordCredentialRoot>
                .None);

        var result = await _application.GetPasswordCredentialAsync(_caller.Object, CancellationToken.None);

        result.Should().BeError(ErrorCode.EntityNotFound);
    }

    [Fact]
    public async Task WhenGetPasswordCredentialAsync_ThenReturnsCredentials()
    {
        _caller.Setup(cc => cc.CallerId)
            .Returns("auserid");
        var credential = CreateVerifiedCredential();
        _repository.Setup(s =>
                s.FindCredentialsByUserIdAsync(It.IsAny<Identifier>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(credential.ToOptional());
        _endUsersService.Setup(eus =>
                eus.GetUserPrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EndUser
            {
                Id = "auserid",
                Classification = EndUserClassification.Person
            });

        var result = await _application.GetPasswordCredentialAsync(_caller.Object, CancellationToken.None);

        result.Value.Id.Should().Be("anid");

        result.Value.IsMfaEnabled.Should().BeFalse();
        _endUsersService.Verify(eus =>
            eus.GetUserPrivateAsync(_caller.Object, "auserid",
                It.IsAny<CancellationToken>()));
    }

    private PasswordCredentialRoot CreateUnVerifiedCredential()
    {
        var credential = CreateCredential();
        credential.SetPasswordCredential("apassword");
        credential.SetRegistrationDetails(EmailAddress.Create("auser@company.com").Value,
            PersonDisplayName.Create("aname").Value);
        credential.InitiateRegistrationVerification();

        return credential;
    }

    private PasswordCredentialRoot CreateVerifiedCredential()
    {
        var credential = CreateUnVerifiedCredential();
        credential.VerifyRegistration();
        return credential;
    }

    private PasswordCredentialRoot CreateCredential()
    {
        return PasswordCredentialRoot.Create(_recorder.Object, _idFactory.Object, _settings.Object,
            _emailAddressService.Object, _tokensService.Object, _encryptionService.Object,
            _passwordHasherService.Object,
            _mfaService.Object, "auserid".ToId()).Value;
    }
}