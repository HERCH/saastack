using Application.Interfaces;
using Application.Resources.Shared;
using Application.Services.Shared;
using Common;
using Common.Configuration;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Authorization;
using Domain.Interfaces.Entities;
using Domain.Services.Shared.DomainServices;
using Domain.Shared;
using Domain.Shared.EndUsers;
using EndUsersApplication.Persistence;
using EndUsersDomain;
using FluentAssertions;
using Moq;
using UnitTesting.Common;
using Xunit;
using Membership = EndUsersDomain.Membership;
using OrganizationOwnership = Domain.Shared.Organizations.OrganizationOwnership;
using PersonName = Application.Resources.Shared.PersonName;

namespace EndUsersApplication.UnitTests;

[Trait("Category", "Unit")]
public class EndUsersApplicationSpec
{
    private readonly EndUsersApplication _application;
    private readonly Mock<ICallerContext> _caller;
    private readonly Mock<IEndUserRepository> _endUserRepository;
    private readonly Mock<IIdentifierFactory> _idFactory;
    private readonly Mock<IInvitationRepository> _invitationRepository;
    private readonly Mock<INotificationsService> _notificationsService;
    private readonly Mock<IRecorder> _recorder;
    private readonly Mock<IUserProfilesService> _userProfilesService;

    public EndUsersApplicationSpec()
    {
        _recorder = new Mock<IRecorder>();
        _caller = new Mock<ICallerContext>();
        _idFactory = new Mock<IIdentifierFactory>();
        var membershipCounter = 0;
        _idFactory.Setup(idf => idf.Create(It.IsAny<IIdentifiableEntity>()))
            .Returns((IIdentifiableEntity entity) =>
            {
                if (entity is Membership)
                {
                    return $"amembershipid{membershipCounter++}".ToId();
                }

                return "anid".ToId();
            });
        var settings = new Mock<IConfigurationSettings>();
        settings.Setup(
                s => s.Platform.GetString(EndUsersApplication.PermittedOperatorsSettingName, It.IsAny<string?>()))
            .Returns("");
        _endUserRepository = new Mock<IEndUserRepository>();
        _endUserRepository.Setup(rep => rep.SaveAsync(It.IsAny<EndUserRoot>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EndUserRoot root, CancellationToken _) => root);
        _endUserRepository.Setup(rep =>
                rep.SaveAsync(It.IsAny<EndUserRoot>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EndUserRoot root, bool _, CancellationToken _) => root);
        _invitationRepository = new Mock<IInvitationRepository>();
        _userProfilesService = new Mock<IUserProfilesService>();
        _notificationsService = new Mock<INotificationsService>();

        _application =
            new EndUsersApplication(_recorder.Object, _idFactory.Object, settings.Object,
                _notificationsService.Object, _userProfilesService.Object,
                _invitationRepository.Object, _endUserRepository.Object);
    }

    [Fact]
    public async Task WhenGetPersonAndUnregistered_ThenReturnsUser()
    {
        var user = EndUserRoot.Create(_recorder.Object, _idFactory.Object, UserClassification.Person).Value;
        _endUserRepository.Setup(rep => rep.LoadAsync(It.IsAny<Identifier>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _application.GetUserAsync(_caller.Object, "anid", CancellationToken.None);

        result.Should().BeSuccess();
        result.Value.Id.Should().Be("anid");
        result.Value.Access.Should().Be(EndUserAccess.Enabled);
        result.Value.Status.Should().Be(EndUserStatus.Unregistered);
        result.Value.Classification.Should().Be(EndUserClassification.Person);
        result.Value.Roles.Should().BeEmpty();
        result.Value.Features.Should().BeEmpty();
    }

    [Fact]
    public async Task WhenRegisterPersonAsyncAndNotAcceptedTerms_ThenReturnsError()
    {
        var result = await _application.RegisterPersonAsync(_caller.Object, null, "auser@company.com",
            "afirstname",
            "alastname",
            "atimezone", "acountrycode", false, CancellationToken.None);

        result.Should().BeError(ErrorCode.RuleViolation, Resources.EndUsersApplication_NotAcceptedTerms);
    }

    [Fact]
    public async Task WhenRegisterPersonAsyncAndWasInvitedAsGuest_ThenCompletesRegistration()
    {
        _caller.Setup(cc => cc.CallId)
            .Returns("acallid");
        var invitee = EndUserRoot.Create(_recorder.Object, _idFactory.Object, UserClassification.Person).Value;
        _userProfilesService.Setup(ups =>
                ups.FindPersonByEmailAddressPrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Optional<UserProfile>.None);
        _invitationRepository.Setup(rep =>
                rep.FindInvitedGuestByEmailAddressAsync(It.IsAny<EmailAddress>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitee.ToOptional());
        _userProfilesService.Setup(ups =>
                ups.GetProfilePrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfile
            {
                Id = "aprofileid",
                Classification = UserProfileClassification.Person,
                UserId = "apersonid",
                DisplayName = "afirstname",
                Name = new PersonName
                {
                    FirstName = "afirstname",
                    LastName = "alastname"
                },
                EmailAddress = "auser@company.com",
                Timezone = "atimezone",
                Address =
                {
                    CountryCode = "acountrycode"
                }
            });
        _endUserRepository.Setup(rep =>
                rep.SaveAsync(It.IsAny<EndUserRoot>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EndUserRoot root, bool _, CancellationToken _) =>
            {
                // HACK: By this time, domain events have created the default membership
                root.AddMembership(root, OrganizationOwnership.Personal, "anorganizationid".ToId(), Roles.Empty,
                    Features.Empty);
                return root;
            });

        var result = await _application.RegisterPersonAsync(_caller.Object, null, "auser@company.com",
            "afirstname",
            "alastname", null, null, true, CancellationToken.None);

        result.Should().BeSuccess();
        result.Value.Id.Should().Be("anid");
        result.Value.Access.Should().Be(EndUserAccess.Enabled);
        result.Value.Status.Should().Be(EndUserStatus.Registered);
        result.Value.Classification.Should().Be(EndUserClassification.Person);
        result.Value.Roles.Should().ContainSingle(role => role == PlatformRoles.Standard.Name);
        result.Value.Features.Should().ContainSingle(feat => feat == PlatformFeatures.PaidTrial.Name);
        result.Value.Profile!.Id.Should().Be("aprofileid");
        result.Value.Profile.DefaultOrganizationId.Should().Be("anorganizationid");
        result.Value.Profile.Address.CountryCode.Should().Be("acountrycode");
        result.Value.Profile.Name.FirstName.Should().Be("afirstname");
        result.Value.Profile.Name.LastName.Should().Be("alastname");
        result.Value.Profile.DisplayName.Should().Be("afirstname");
        result.Value.Profile.EmailAddress.Should().Be("auser@company.com");
        result.Value.Profile.Timezone.Should().Be("atimezone");
        _invitationRepository.Verify(rep =>
            rep.FindInvitedGuestByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _userProfilesService.Verify(ups =>
            ups.GetProfilePrivateAsync(It.Is<ICallerContext>(cc =>
                cc.CallId == "acallid"
                && cc.IsServiceAccount
            ), "anid", It.IsAny<CancellationToken>()));
        _notificationsService.Verify(ns => ns.NotifyPasswordRegistrationRepeatCourtesyAsync(It.IsAny<ICallerContext>(),
            It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenRegisterPersonAsyncAndAcceptingGuestInvitation_ThenCompletesRegistration()
    {
        _caller.Setup(cc => cc.CallerId)
            .Returns(CallerConstants.AnonymousUserId);
        _caller.Setup(cc => cc.CallId)
            .Returns("acallid");
        var tokensService = new Mock<ITokensService>();
        tokensService.Setup(ts => ts.CreateGuestInvitationToken())
            .Returns("aninvitationtoken");
        var invitee = EndUserRoot.Create(_recorder.Object, _idFactory.Object, UserClassification.Person).Value;
        await invitee.InviteGuestAsync(tokensService.Object, "aninviterid".ToId(),
            EmailAddress.Create("auser@company.com").Value, (_, _) => Task.FromResult(Result.Ok));
        _invitationRepository.Setup(rep =>
                rep.FindInvitedGuestByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitee.ToOptional());
        _userProfilesService.Setup(ups =>
                ups.FindPersonByEmailAddressPrivateAsync(It.IsAny<ICallerContext>(), "auser@company.com",
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Optional<UserProfile>.None);
        _userProfilesService.Setup(ups =>
                ups.FindPersonByEmailAddressPrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Optional<UserProfile>.None);
        _invitationRepository.Setup(rep =>
                rep.FindInvitedGuestByEmailAddressAsync(It.IsAny<EmailAddress>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitee.ToOptional());
        _userProfilesService.Setup(ups =>
                ups.GetProfilePrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfile
            {
                Id = "aprofileid",
                Classification = UserProfileClassification.Person,
                UserId = "apersonid",
                DisplayName = "afirstname",
                Name = new PersonName
                {
                    FirstName = "afirstname",
                    LastName = "alastname"
                },
                EmailAddress = "auser@company.com",
                Timezone = "atimezone",
                Address =
                {
                    CountryCode = "acountrycode"
                }
            });
        _endUserRepository.Setup(rep =>
                rep.SaveAsync(It.IsAny<EndUserRoot>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EndUserRoot root, bool _, CancellationToken _) =>
            {
                // HACK: By this time, domain events have created the default membership
                root.AddMembership(root, OrganizationOwnership.Personal, "anorganizationid".ToId(), Roles.Empty,
                    Features.Empty);
                return root;
            });

        var result = await _application.RegisterPersonAsync(_caller.Object, "aninvitationtoken", "auser@company.com",
            "afirstname", "alastname", null, null, true, CancellationToken.None);

        result.Should().BeSuccess();
        result.Value.Id.Should().Be("anid");
        result.Value.Access.Should().Be(EndUserAccess.Enabled);
        result.Value.Status.Should().Be(EndUserStatus.Registered);
        result.Value.Classification.Should().Be(EndUserClassification.Person);
        result.Value.Roles.Should().ContainSingle(role => role == PlatformRoles.Standard.Name);
        result.Value.Features.Should().ContainSingle(feat => feat == PlatformFeatures.PaidTrial.Name);
        result.Value.Profile!.Id.Should().Be("aprofileid");
        result.Value.Profile.DefaultOrganizationId.Should().Be("anorganizationid");
        result.Value.Profile.Address.CountryCode.Should().Be("acountrycode");
        result.Value.Profile.Name.FirstName.Should().Be("afirstname");
        result.Value.Profile.Name.LastName.Should().Be("alastname");
        result.Value.Profile.DisplayName.Should().Be("afirstname");
        result.Value.Profile.EmailAddress.Should().Be("auser@company.com");
        result.Value.Profile.Timezone.Should().Be("atimezone");
        _invitationRepository.Verify(rep =>
            rep.FindInvitedGuestByTokenAsync("aninvitationtoken", It.IsAny<CancellationToken>()));
        _userProfilesService.Verify(ups =>
            ups.GetProfilePrivateAsync(It.Is<ICallerContext>(cc =>
                cc.CallId == "acallid"
                && cc.IsServiceAccount
            ), "anid", It.IsAny<CancellationToken>()));
        _notificationsService.Verify(ns => ns.NotifyPasswordRegistrationRepeatCourtesyAsync(It.IsAny<ICallerContext>(),
            It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenRegisterPersonAsyncAndAcceptingAnUnknownInvitation_ThenRegisters()
    {
        _caller.Setup(cc => cc.CallId)
            .Returns("acallid");
        _invitationRepository.Setup(rep =>
                rep.FindInvitedGuestByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Optional<EndUserRoot>.None);
        var invitee = EndUserRoot.Create(_recorder.Object, _idFactory.Object, UserClassification.Person).Value;
        _userProfilesService.Setup(ups =>
                ups.FindPersonByEmailAddressPrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Optional<UserProfile>.None);
        _invitationRepository.Setup(rep =>
                rep.FindInvitedGuestByEmailAddressAsync(It.IsAny<EmailAddress>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitee.ToOptional());
        _userProfilesService.Setup(ups =>
                ups.GetProfilePrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfile
            {
                Id = "aprofileid",
                Classification = UserProfileClassification.Person,
                UserId = "apersonid",
                DisplayName = "afirstname",
                Name = new PersonName
                {
                    FirstName = "afirstname",
                    LastName = "alastname"
                },
                EmailAddress = "auser@company.com",
                Timezone = "atimezone",
                Address =
                {
                    CountryCode = "acountrycode"
                }
            });
        _endUserRepository.Setup(rep =>
                rep.SaveAsync(It.IsAny<EndUserRoot>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EndUserRoot root, bool _, CancellationToken _) =>
            {
                // HACK: By this time, domain events have created the default membership
                root.AddMembership(root, OrganizationOwnership.Personal, "anorganizationid".ToId(), Roles.Empty,
                    Features.Empty);
                return root;
            });

        var result = await _application.RegisterPersonAsync(_caller.Object, "anunknowninvitationtoken",
            "auser@company.com", "afirstname", "alastname", null, null, true, CancellationToken.None);

        result.Should().BeSuccess();
        result.Value.Id.Should().Be("anid");
        result.Value.Access.Should().Be(EndUserAccess.Enabled);
        result.Value.Status.Should().Be(EndUserStatus.Registered);
        result.Value.Classification.Should().Be(EndUserClassification.Person);
        result.Value.Roles.Should().ContainSingle(role => role == PlatformRoles.Standard.Name);
        result.Value.Features.Should().ContainSingle(feat => feat == PlatformFeatures.PaidTrial.Name);
        result.Value.Profile!.Id.Should().Be("aprofileid");
        result.Value.Profile.DefaultOrganizationId.Should().Be("anorganizationid");
        result.Value.Profile.Address.CountryCode.Should().Be("acountrycode");
        result.Value.Profile.Name.FirstName.Should().Be("afirstname");
        result.Value.Profile.Name.LastName.Should().Be("alastname");
        result.Value.Profile.DisplayName.Should().Be("afirstname");
        result.Value.Profile.EmailAddress.Should().Be("auser@company.com");
        result.Value.Profile.Timezone.Should().Be("atimezone");
        _invitationRepository.Verify(rep =>
            rep.FindInvitedGuestByTokenAsync("anunknowninvitationtoken", It.IsAny<CancellationToken>()));
        _userProfilesService.Verify(ups =>
            ups.GetProfilePrivateAsync(It.Is<ICallerContext>(cc =>
                cc.CallId == "acallid"
                && cc.IsServiceAccount
            ), "anid", It.IsAny<CancellationToken>()));
        _notificationsService.Verify(ns => ns.NotifyPasswordRegistrationRepeatCourtesyAsync(It.IsAny<ICallerContext>(),
            It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task
        WhenRegisterPersonAsyncAndAcceptingGuestInvitationWithExistingPersonsEmailAddress_ThenReturnsError()
    {
        _caller.Setup(cc => cc.CallerId)
            .Returns(CallerConstants.AnonymousUserId);
        var invitee = EndUserRoot.Create(_recorder.Object, _idFactory.Object, UserClassification.Person).Value;
        _invitationRepository.Setup(rep =>
                rep.FindInvitedGuestByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitee.ToOptional());
        var otherUser = EndUserRoot.Create(_recorder.Object, _idFactory.Object, UserClassification.Person).Value;
        _endUserRepository.Setup(rep => rep.LoadAsync(It.IsAny<Identifier>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherUser);
        _userProfilesService.Setup(ups =>
                ups.FindPersonByEmailAddressPrivateAsync(It.IsAny<ICallerContext>(), "auser@company.com",
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfile
            {
                Id = "anotherprofileid",
                Classification = UserProfileClassification.Person,
                UserId = "anotherpersonid",
                DisplayName = "afirstname",
                Name = new PersonName
                {
                    FirstName = "afirstname",
                    LastName = "alastname"
                },
                EmailAddress = "anotheruser@company.com"
            }.ToOptional());
        _invitationRepository.Setup(rep =>
                rep.FindInvitedGuestByEmailAddressAsync(It.IsAny<EmailAddress>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitee.ToOptional());

        var result = await _application.RegisterPersonAsync(_caller.Object, "aninvitationtoken", "auser@company.com",
            "afirstname", "alastname", null, null, true, CancellationToken.None);

        result.Should().BeError(ErrorCode.EntityExists,
            Resources.EndUsersApplication_AcceptedInvitationWithExistingEmailAddress);
        _invitationRepository.Verify(rep =>
            rep.FindInvitedGuestByTokenAsync("aninvitationtoken", It.IsAny<CancellationToken>()));
        _userProfilesService.Setup(ups =>
            ups.FindPersonByEmailAddressPrivateAsync(_caller.Object, "auser@company.com",
                It.IsAny<CancellationToken>()));
        _userProfilesService.Verify(ups =>
                ups.GetProfilePrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
        _notificationsService.Verify(
            ns => ns.NotifyPasswordRegistrationRepeatCourtesyAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenRegisterPersonAsyncAndAlreadyRegistered_ThenSendsCourtesyEmail()
    {
        var endUser = EndUserRoot.Create(_recorder.Object, _idFactory.Object, UserClassification.Person).Value;
        endUser.Register(Roles.Empty, Features.Empty, EndUserProfile.Create("afirstname").Value,
            EmailAddress.Create("auser@company.com").Value);
        _userProfilesService.Setup(ups =>
                ups.FindPersonByEmailAddressPrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfile
            {
                Id = "aprofileid",
                Classification = UserProfileClassification.Person,
                UserId = "auserid",
                DisplayName = "afirstname",
                Name = new PersonName
                {
                    FirstName = "afirstname",
                    LastName = "alastname"
                },
                EmailAddress = "anotheruser@company.com",
                Address =
                {
                    CountryCode = "acountrycode"
                },
                Timezone = "atimezone"
            }.ToOptional());
        endUser.AddMembership(endUser, OrganizationOwnership.Shared, "anorganizationid".ToId(), Roles.Empty,
            Features.Empty);
        _endUserRepository.Setup(rep =>
                rep.LoadAsync(It.IsAny<Identifier>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(endUser);
        _notificationsService.Setup(ns =>
                ns.NotifyPasswordRegistrationRepeatCourtesyAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok);

        var result = await _application.RegisterPersonAsync(_caller.Object, null, "auser@company.com",
            "afirstname",
            "alastname", null, null, true, CancellationToken.None);

        result.Should().BeSuccess();
        result.Value.Id.Should().Be("anid");
        result.Value.Access.Should().Be(EndUserAccess.Enabled);
        result.Value.Status.Should().Be(EndUserStatus.Registered);
        result.Value.Classification.Should().Be(EndUserClassification.Person);
        result.Value.Roles.Should().BeEmpty();
        result.Value.Features.Should().BeEmpty();
        result.Value.Profile!.Id.Should().Be("aprofileid");
        result.Value.Profile.DefaultOrganizationId.Should().Be("amembershipid0");
        result.Value.Profile.Address.CountryCode.Should().Be("acountrycode");
        result.Value.Profile.Name.FirstName.Should().Be("afirstname");
        result.Value.Profile.Name.LastName.Should().Be("alastname");
        result.Value.Profile.DisplayName.Should().Be("afirstname");
        result.Value.Profile.EmailAddress.Should().Be("anotheruser@company.com");
        result.Value.Profile.Timezone.Should().Be("atimezone");
        _invitationRepository.Verify(rep =>
            rep.FindInvitedGuestByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _userProfilesService.Verify(ups =>
                ups.GetProfilePrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
        _notificationsService.Verify(ns => ns.NotifyPasswordRegistrationRepeatCourtesyAsync(_caller.Object, "anid",
            "anotheruser@company.com", "afirstname", "atimezone", "acountrycode", CancellationToken.None));
    }

    [Fact]
    public async Task WhenRegisterPersonAsyncAndNeverRegisteredNorInvitedAsGuest_ThenRegisters()
    {
        _caller.Setup(cc => cc.CallId)
            .Returns("acallid");
        _userProfilesService.Setup(ups =>
                ups.FindPersonByEmailAddressPrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Optional<UserProfile>.None);
        _invitationRepository.Setup(rep =>
                rep.FindInvitedGuestByEmailAddressAsync(It.IsAny<EmailAddress>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Optional<EndUserRoot>.None);
        _userProfilesService.Setup(ups =>
                ups.GetProfilePrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfile
            {
                Id = "aprofileid",
                Classification = UserProfileClassification.Person,
                UserId = "apersonid",
                DisplayName = "afirstname",
                Name = new PersonName
                {
                    FirstName = "afirstname",
                    LastName = "alastname"
                },
                EmailAddress = "auser@company.com",
                Timezone = "atimezone",
                Address =
                {
                    CountryCode = "acountrycode"
                }
            });
        _endUserRepository.Setup(rep =>
                rep.SaveAsync(It.IsAny<EndUserRoot>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EndUserRoot root, bool _, CancellationToken _) =>
            {
                // HACK: By this time, domain events have created the default membership
                root.AddMembership(root, OrganizationOwnership.Personal, "anorganizationid".ToId(), Roles.Empty,
                    Features.Empty);
                return root;
            });

        var result = await _application.RegisterPersonAsync(_caller.Object, null, "auser@company.com",
            "afirstname",
            "alastname", null, null, true, CancellationToken.None);

        result.Should().BeSuccess();
        result.Value.Id.Should().Be("anid");
        result.Value.Access.Should().Be(EndUserAccess.Enabled);
        result.Value.Status.Should().Be(EndUserStatus.Registered);
        result.Value.Classification.Should().Be(EndUserClassification.Person);
        result.Value.Roles.Should().ContainSingle(role => role == PlatformRoles.Standard.Name);
        result.Value.Features.Should().ContainSingle(feat => feat == PlatformFeatures.PaidTrial.Name);
        result.Value.Profile!.Id.Should().Be("aprofileid");
        result.Value.Profile.DefaultOrganizationId.Should().Be("anorganizationid");
        result.Value.Profile.Address.CountryCode.Should().Be("acountrycode");
        result.Value.Profile.Name.FirstName.Should().Be("afirstname");
        result.Value.Profile.Name.LastName.Should().Be("alastname");
        result.Value.Profile.DisplayName.Should().Be("afirstname");
        result.Value.Profile.EmailAddress.Should().Be("auser@company.com");
        result.Value.Profile.Timezone.Should().Be("atimezone");
        _invitationRepository.Verify(rep =>
            rep.FindInvitedGuestByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _userProfilesService.Verify(ups =>
            ups.GetProfilePrivateAsync(It.Is<ICallerContext>(cc =>
                cc.CallId == "acallid"
                && cc.IsServiceAccount
            ), "anid", It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task WhenRegisterMachineAsyncByAnonymousUser_ThenRegistersWithNoFeatures()
    {
        _userProfilesService.Setup(ups =>
                ups.GetProfilePrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfile
            {
                Id = "aprofileid",
                Classification = UserProfileClassification.Machine,
                UserId = "amachineid",
                DisplayName = "amachinename",
                Name = new PersonName
                {
                    FirstName = "amachinename"
                },
                Timezone = "atimezone",
                Address =
                {
                    CountryCode = "acountrycode"
                }
            });
        _caller.Setup(cc => cc.IsAuthenticated)
            .Returns(false);
        _caller.Setup(cc => cc.CallId)
            .Returns("acallid");
        _endUserRepository.Setup(rep =>
                rep.SaveAsync(It.IsAny<EndUserRoot>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EndUserRoot root, bool _, CancellationToken _) =>
            {
                // HACK: By this time, domain events have created the default membership
                root.AddMembership(root, OrganizationOwnership.Personal, "anorganizationid".ToId(), Roles.Empty,
                    Features.Empty);
                return root;
            });

        var result = await _application.RegisterMachineAsync(_caller.Object, "aname", Timezones.Default.ToString(),
            CountryCodes.Default.ToString(), CancellationToken.None);

        result.Should().BeSuccess();
        result.Value.Id.Should().Be("anid");
        result.Value.Access.Should().Be(EndUserAccess.Enabled);
        result.Value.Status.Should().Be(EndUserStatus.Registered);
        result.Value.Classification.Should().Be(EndUserClassification.Machine);
        result.Value.Roles.Should().ContainSingle(role => role == PlatformRoles.Standard.Name);
        result.Value.Features.Should().ContainSingle(feat => feat == PlatformFeatures.Basic.Name);
        result.Value.Profile!.Id.Should().Be("aprofileid");
        result.Value.Profile.DefaultOrganizationId.Should().Be("anorganizationid");
        result.Value.Profile.Address.CountryCode.Should().Be("acountrycode");
        result.Value.Profile.Name.FirstName.Should().Be("amachinename");
        result.Value.Profile.Name.LastName.Should().BeNull();
        result.Value.Profile.DisplayName.Should().Be("amachinename");
        result.Value.Profile.EmailAddress.Should().BeNull();
        result.Value.Profile.Timezone.Should().Be("atimezone");
        _userProfilesService.Verify(ups =>
            ups.GetProfilePrivateAsync(It.Is<ICallerContext>(cc =>
                cc.CallId == "acallid"
                && cc.IsServiceAccount
            ), "anid", It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task WhenRegisterMachineAsyncByAuthenticatedUser_ThenRegistersWithBasicFeatures()
    {
        _caller.Setup(cc => cc.IsAuthenticated)
            .Returns(true);
        _caller.Setup(cc => cc.CallId)
            .Returns("acallid");
        var adder = EndUserRoot.Create(_recorder.Object, _idFactory.Object, UserClassification.Person).Value;
        _endUserRepository.Setup(rep => rep.LoadAsync(It.IsAny<Identifier>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(adder);
        adder.Register(Roles.Empty, Features.Empty, EndUserProfile.Create("afirstname").Value,
            EmailAddress.Create("auser@company.com").Value);
        adder.AddMembership(adder, OrganizationOwnership.Shared, "anotherorganizationid".ToId(), Roles.Empty,
            Features.Empty);
        _userProfilesService.Setup(ups =>
                ups.GetProfilePrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfile
            {
                Id = "aprofileid",
                Classification = UserProfileClassification.Machine,
                UserId = "amachineid",
                DisplayName = "amachinename",
                Name = new PersonName
                {
                    FirstName = "amachinename"
                },
                Timezone = "atimezone",
                Address =
                {
                    CountryCode = "acountrycode"
                }
            });

        var result = await _application.RegisterMachineAsync(_caller.Object, "aname", Timezones.Default.ToString(),
            CountryCodes.Default.ToString(), CancellationToken.None);

        result.Should().BeSuccess();
        result.Value.Id.Should().Be("anid");
        result.Value.Access.Should().Be(EndUserAccess.Enabled);
        result.Value.Status.Should().Be(EndUserStatus.Registered);
        result.Value.Classification.Should().Be(EndUserClassification.Machine);
        result.Value.Roles.Should().ContainSingle(role => role == PlatformRoles.Standard.Name);
        result.Value.Features.Should().ContainSingle(feat => feat == PlatformFeatures.PaidTrial.Name);
        result.Value.Profile!.Id.Should().Be("aprofileid");
        result.Value.Profile.DefaultOrganizationId.Should().Be("anotherorganizationid");
        result.Value.Profile.Address.CountryCode.Should().Be("acountrycode");
        result.Value.Profile.Name.FirstName.Should().Be("amachinename");
        result.Value.Profile.Name.LastName.Should().BeNull();
        result.Value.Profile.DisplayName.Should().Be("amachinename");
        result.Value.Profile.EmailAddress.Should().BeNull();
        result.Value.Profile.Timezone.Should().Be("atimezone");
        _userProfilesService.Verify(ups =>
            ups.GetProfilePrivateAsync(It.Is<ICallerContext>(cc =>
                cc.CallId == "acallid"
                && cc.IsServiceAccount
            ), "anid", It.IsAny<CancellationToken>()));
    }

#if TESTINGONLY
    [Fact]
    public async Task WhenAssignPlatformRolesAsync_ThenAssigns()
    {
        _caller.Setup(cc => cc.CallerId)
            .Returns("anassignerid");
        var assignee = EndUserRoot.Create(_recorder.Object, _idFactory.Object, UserClassification.Person).Value;
        assignee.Register(Roles.Create(PlatformRoles.Standard).Value, Features.Create(PlatformFeatures.Basic).Value,
            EndUserProfile.Create("afirstname").Value, Optional<EmailAddress>.None);
        _endUserRepository.Setup(rep => rep.LoadAsync("anassigneeid".ToId(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignee);
        var assigner = EndUserRoot.Create(_recorder.Object, _idFactory.Object, UserClassification.Person).Value;
        assigner.Register(Roles.Create(PlatformRoles.Operations).Value, Features.Create(),
            EndUserProfile.Create("afirstname").Value, Optional<EmailAddress>.None);
        _endUserRepository.Setup(rep => rep.LoadAsync("anassignerid".ToId(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(assigner);

        var result = await _application.AssignPlatformRolesAsync(_caller.Object, "anassigneeid",
            [PlatformRoles.TestingOnly.Name],
            CancellationToken.None);

        result.Should().BeSuccess();
        result.Value.Roles.Should().ContainInOrder(PlatformRoles.Standard.Name, PlatformRoles.TestingOnly.Name);
    }
#endif

#if TESTINGONLY
    [Fact]
    public async Task WhenUnassignPlatformRolesAsync_ThenUnassigns()
    {
        _caller.Setup(cc => cc.CallerId)
            .Returns("anassignerid");
        var assignee = EndUserRoot.Create(_recorder.Object, _idFactory.Object, UserClassification.Person).Value;
        assignee.Register(Roles.Create(PlatformRoles.Standard, PlatformRoles.TestingOnly).Value,
            Features.Create(PlatformFeatures.Basic).Value, EndUserProfile.Create("afirstname").Value,
            Optional<EmailAddress>.None);
        _endUserRepository.Setup(rep => rep.LoadAsync("anassigneeid".ToId(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignee);
        var assigner = EndUserRoot.Create(_recorder.Object, _idFactory.Object, UserClassification.Person).Value;
        assigner.Register(Roles.Create(PlatformRoles.Operations).Value, Features.Create(),
            EndUserProfile.Create("afirstname").Value, Optional<EmailAddress>.None);
        _endUserRepository.Setup(rep => rep.LoadAsync("anassignerid".ToId(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(assigner);

        var result = await _application.UnassignPlatformRolesAsync(_caller.Object, "anassigneeid",
            [PlatformRoles.TestingOnly.Name],
            CancellationToken.None);

        result.Should().BeSuccess();
        result.Value.Roles.Should().ContainSingle(PlatformRoles.Standard.Name);
    }
#endif

#if TESTINGONLY
    [Fact]
    public async Task WhenAssignTenantRolesAsync_ThenAssigns()
    {
        _caller.Setup(cc => cc.CallerId)
            .Returns("anassignerid");
        var assigner = EndUserRoot.Create(_recorder.Object, _idFactory.Object, UserClassification.Person).Value;
        assigner.Register(Roles.Create(PlatformRoles.Operations).Value, Features.Create(),
            EndUserProfile.Create("afirstname").Value, Optional<EmailAddress>.None);
        assigner.AddMembership(assigner, OrganizationOwnership.Shared, "anorganizationid".ToId(),
            Roles.Create(TenantRoles.Owner).Value,
            Features.Create(TenantFeatures.Basic).Value);
        _endUserRepository.Setup(rep => rep.LoadAsync("anassignerid".ToId(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(assigner);
        var assignee = EndUserRoot.Create(_recorder.Object, _idFactory.Object, UserClassification.Person).Value;
        assignee.Register(Roles.Create(PlatformRoles.Standard).Value, Features.Create(PlatformFeatures.Basic).Value,
            EndUserProfile.Create("afirstname").Value, Optional<EmailAddress>.None);
        assignee.AddMembership(assignee, OrganizationOwnership.Shared, "anorganizationid".ToId(),
            Roles.Create(TenantRoles.Member).Value,
            Features.Create(TenantFeatures.Basic).Value);
        _endUserRepository.Setup(rep => rep.LoadAsync("anassigneeid".ToId(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignee);

        var result = await _application.AssignTenantRolesAsync(_caller.Object, "anorganizationid", "anassigneeid",
            [TenantRoles.TestingOnly.Name],
            CancellationToken.None);

        result.Should().BeSuccess();
        result.Value.Roles.Should().ContainInOrder(PlatformRoles.Standard.Name);
        result.Value.Memberships[0].Roles.Should()
            .ContainInOrder(TenantRoles.Member.Name, TenantRoles.TestingOnly.Name);
    }
#endif

    [Fact]
    public async Task WhenFindPersonByEmailAsyncAndNotExists_ThenReturnsNone()
    {
        _userProfilesService.Setup(ups =>
                ups.FindPersonByEmailAddressPrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Optional<UserProfile>.None);
        _invitationRepository.Setup(rep =>
                rep.FindInvitedGuestByEmailAddressAsync(It.IsAny<EmailAddress>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Optional.None<EndUserRoot>());

        var result =
            await _application.FindPersonByEmailAddressAsync(_caller.Object, "auser@company.com",
                CancellationToken.None);

        result.Should().BeSuccess();
        result.Value.Should().BeNone();
    }

    [Fact]
    public async Task WhenFindPersonByEmailAsyncAndExists_ThenReturns()
    {
        var endUser = EndUserRoot.Create(_recorder.Object, _idFactory.Object, UserClassification.Person).Value;
        _userProfilesService.Setup(ups =>
                ups.FindPersonByEmailAddressPrivateAsync(It.IsAny<ICallerContext>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Optional<UserProfile>.None);
        _invitationRepository.Setup(rep =>
                rep.FindInvitedGuestByEmailAddressAsync(It.IsAny<EmailAddress>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(endUser.ToOptional());

        var result =
            await _application.FindPersonByEmailAddressAsync(_caller.Object, "auser@company.com",
                CancellationToken.None);

        result.Should().BeSuccess();
        result.Value.Value.Id.Should().Be("anid");
    }

    [Fact]
    public async Task WhenGetMembershipsAndNotRegisteredOrMemberAsync_ThenReturnsUser()
    {
        var user = EndUserRoot.Create(_recorder.Object, _idFactory.Object, UserClassification.Person).Value;
        _endUserRepository.Setup(rep => rep.LoadAsync(It.IsAny<Identifier>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _application.GetMembershipsAsync(_caller.Object, "anid", CancellationToken.None);

        result.Should().BeSuccess();
        result.Value.Id.Should().Be("anid");
        result.Value.Access.Should().Be(EndUserAccess.Enabled);
        result.Value.Status.Should().Be(EndUserStatus.Unregistered);
        result.Value.Classification.Should().Be(EndUserClassification.Person);
        result.Value.Roles.Should().BeEmpty();
        result.Value.Features.Should().BeEmpty();
        result.Value.Memberships.Count.Should().Be(0);
    }

    [Fact]
    public async Task WhenGetMembershipsAsync_ThenReturnsUser()
    {
        var user = EndUserRoot.Create(_recorder.Object, _idFactory.Object, UserClassification.Person).Value;
        user.Register(Roles.Create(PlatformRoles.Standard).Value, Features.Create(PlatformFeatures.Basic).Value,
            EndUserProfile.Create("afirstname").Value, EmailAddress.Create("auser@company.com").Value);
        user.AddMembership(user, OrganizationOwnership.Shared, "anorganizationid".ToId(),
            Roles.Create(TenantRoles.Member).Value,
            Features.Create(TenantFeatures.PaidTrial).Value);
        _endUserRepository.Setup(rep => rep.LoadAsync(It.IsAny<Identifier>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _application.GetMembershipsAsync(_caller.Object, "anid", CancellationToken.None);

        result.Should().BeSuccess();
        result.Value.Id.Should().Be("anid");
        result.Value.Access.Should().Be(EndUserAccess.Enabled);
        result.Value.Status.Should().Be(EndUserStatus.Registered);
        result.Value.Classification.Should().Be(EndUserClassification.Person);
        result.Value.Roles.Should().ContainSingle(role => role == PlatformRoles.Standard.Name);
        result.Value.Features.Should().ContainSingle(feat => feat == PlatformFeatures.Basic.Name);
        result.Value.Memberships.Count.Should().Be(1);
        result.Value.Memberships[0].IsDefault.Should().BeTrue();
        result.Value.Memberships[0].OrganizationId.Should().Be("anorganizationid");
        result.Value.Memberships[0].Roles.Should().ContainSingle(role => role == TenantRoles.Member.Name);
        result.Value.Memberships[0].Features.Should().ContainSingle(feat => feat == TenantFeatures.PaidTrial.Name);
    }
}