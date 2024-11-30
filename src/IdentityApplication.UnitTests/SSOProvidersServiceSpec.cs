using Application.Interfaces;
using Application.Resources.Shared;
using Common;
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
using JetBrains.Annotations;
using Moq;
using UnitTesting.Common;
using Xunit;

namespace IdentityApplication.UnitTests;

[UsedImplicitly]
public class SSOProvidersServiceSpec
{
    [Trait("Category", "Unit")]
    public class GivenNoAuthProviders
    {
        private readonly Mock<ICallerContext> _caller;
        private readonly SSOProvidersService _service;

        public GivenNoAuthProviders()
        {
            _caller = new Mock<ICallerContext>();
            var recorder = new Mock<IRecorder>();
            var idFactory = new Mock<IIdentifierFactory>();
            var repository = new Mock<ISSOUsersRepository>();
            var encryptionService = new Mock<IEncryptionService>();

            _service = new SSOProvidersService(recorder.Object, idFactory.Object, encryptionService.Object,
                new List<ISSOAuthenticationProvider>(),
                repository.Object);
        }

        [Fact]
        public async Task WhenFindByNameAsyncAndNotRegistered_ThenReturnsNone()
        {
            var result = await _service.FindByProviderNameAsync("aname", CancellationToken.None);

            result.Should().BeSuccess();
            result.Value.Should().BeNone();
        }

        [Fact]
        public async Task WhenSaveUserInfoAsyncAndProviderNotRegistered_ThenReturnsError()
        {
            var userInfo = new SSOUserInfo(new List<AuthToken>(), "auser@company.com", "afirstname", null,
                Timezones.Default, CountryCodes.Default);

            var result =
                await _service.SaveUserInfoAsync(_caller.Object, "aprovidername", "auserid".ToId(), userInfo,
                    CancellationToken.None);

            result.Should().BeError(ErrorCode.EntityNotFound,
                Resources.SSOProvidersService_UnknownProvider.Format("aprovidername"));
        }
    }

    [Trait("Category", "Unit")]
    public class GivenAuthProviders
    {
        private readonly SSOProvidersService _service;

        public GivenAuthProviders()
        {
            var recorder = new Mock<IRecorder>();
            var idFactory = new Mock<IIdentifierFactory>();
            idFactory.Setup(idf => idf.Create(It.IsAny<IIdentifiableEntity>()))
                .Returns("anid".ToId());
            var repository = new Mock<ISSOUsersRepository>();
            var encryptionService = new Mock<IEncryptionService>();

            _service = new SSOProvidersService(recorder.Object, idFactory.Object, encryptionService.Object,
                new List<ISSOAuthenticationProvider>
                {
                    new TestSSOAuthenticationProvider()
                },
                repository.Object);
        }

        [Fact]
        public async Task WhenFindByNameAsyncAndNotRegistered_ThenReturnsNone()
        {
            var result = await _service.FindByProviderNameAsync("aname", CancellationToken.None);

            result.Should().BeSuccess();
            result.Value.Should().BeNone();
        }

        [Fact]
        public async Task WhenFindByNameAsyncRegistered_ThenReturnsProvider()
        {
            var result =
                await _service.FindByProviderNameAsync(TestSSOAuthenticationProvider.Name, CancellationToken.None);

            result.Should().BeSuccess();
            result.Value.Value.Should().BeOfType<TestSSOAuthenticationProvider>();
        }
    }

    [Trait("Category", "Unit")]
    public class GivenAnAuthProvider
    {
        private readonly Mock<ICallerContext> _caller;
        private readonly Mock<IEncryptionService> _encryptionService;
        private readonly Mock<IIdentifierFactory> _idFactory;
        private readonly Mock<IRecorder> _recorder;
        private readonly Mock<ISSOUsersRepository> _repository;
        private readonly SSOProvidersService _service;

        public GivenAnAuthProvider()
        {
            _caller = new Mock<ICallerContext>();
            _recorder = new Mock<IRecorder>();
            _idFactory = new Mock<IIdentifierFactory>();
            _idFactory.Setup(idf => idf.Create(It.IsAny<IIdentifiableEntity>()))
                .Returns("anid".ToId());
            _repository = new Mock<ISSOUsersRepository>();
            _repository.Setup(rep => rep.SaveAsync(It.IsAny<SSOUserRoot>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SSOUserRoot root, CancellationToken _) => root);
            _encryptionService = new Mock<IEncryptionService>();
            _encryptionService.Setup(es => es.Encrypt(It.IsAny<string>()))
                .Returns((string _) => "anencryptedvalue");
            _encryptionService.Setup(es => es.Decrypt(It.IsAny<string>()))
                .Returns((string _) => "adecryptedvalue");

            _service = new SSOProvidersService(_recorder.Object, _idFactory.Object, _encryptionService.Object,
                new List<ISSOAuthenticationProvider>
                {
                    new TestSSOAuthenticationProvider()
                },
                _repository.Object);
        }

        [Fact]
        public async Task WhenSaveUserInfoAsyncAndProviderNotRegistered_ThenReturnsError()
        {
            var userInfo = new SSOUserInfo(new List<AuthToken>(), "auser@company.com", "afirstname", null,
                Timezones.Default, CountryCodes.Default);

            var result =
                await _service.SaveUserInfoAsync(_caller.Object, "aprovidername", "auserid".ToId(), userInfo,
                    CancellationToken.None);

            result.Should().BeError(ErrorCode.EntityNotFound,
                Resources.SSOProvidersService_UnknownProvider.Format("aprovidername"));
        }

        [Fact]
        public async Task WhenSaveUserInfoAsyncAndUserNotExists_ThenCreatesAndSavesDetails()
        {
            var userInfo = new SSOUserInfo(new List<AuthToken>(), "auser@company.com", "afirstname", null,
                Timezones.Default, CountryCodes.Default);
            _repository.Setup(rep =>
                    rep.FindByUserIdAsync(It.IsAny<string>(), It.IsAny<Identifier>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(Optional<SSOUserRoot>.None);

            var result =
                await _service.SaveUserInfoAsync(_caller.Object, TestSSOAuthenticationProvider.Name, "auserid".ToId(),
                    userInfo,
                    CancellationToken.None);

            result.Should().BeSuccess();
            _repository.Verify(rep => rep.SaveAsync(It.Is<SSOUserRoot>(user =>
                user.Id == "anid"
                && user.ProviderName == TestSSOAuthenticationProvider.Name
                && user.UserId == "auserid"
                && user.EmailAddress.Value.Address == "auser@company.com"
                && user.Name.Value.FirstName == "afirstname"
                && user.Name.Value.LastName == Optional<Name>.None
                && user.Timezone.Value == Timezones.Default
                && user.Address.Value.CountryCode == CountryCodes.Default
            ), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task WhenSaveUserTokensAsyncAndProviderNotRegistered_ThenReturnsError()
        {
            var tokens = new ProviderAuthenticationTokens
            {
                Provider = "aprovidername",
                AccessToken = new AuthenticationToken
                {
                    ExpiresOn = default,
                    Type = TokenType.AccessToken,
                    Value = "anaccesstoken"
                },
                RefreshToken = new AuthenticationToken
                {
                    ExpiresOn = default,
                    Type = TokenType.RefreshToken,
                    Value = "arefreshtoken"
                },
                OtherTokens =
                [
                    new AuthenticationToken
                    {
                        ExpiresOn = default,
                        Type = TokenType.OtherToken,
                        Value = "anothertoken"
                    }
                ]
            };

            var result =
                await _service.SaveUserTokensAsync(_caller.Object, "aprovidername", "auserid".ToId(), tokens,
                    CancellationToken.None);

            result.Should().BeError(ErrorCode.EntityNotFound,
                Resources.SSOProvidersService_UnknownProvider.Format("aprovidername"));
            _repository.Verify(
                rep => rep.FindByUserIdAsync(It.IsAny<string>(), It.IsAny<Identifier>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task WhenSaveUserTokensAsyncAndUserNotExists_ThenReturnsError()
        {
            var tokens = new ProviderAuthenticationTokens
            {
                Provider = "aprovidername",
                AccessToken = new AuthenticationToken
                {
                    ExpiresOn = default,
                    Type = TokenType.AccessToken,
                    Value = "anaccesstoken"
                },
                RefreshToken = new AuthenticationToken
                {
                    ExpiresOn = default,
                    Type = TokenType.RefreshToken,
                    Value = "arefreshtoken"
                },
                OtherTokens =
                [
                    new AuthenticationToken
                    {
                        ExpiresOn = default,
                        Type = TokenType.OtherToken,
                        Value = "anothertoken"
                    }
                ]
            };
            _repository.Setup(rep =>
                    rep.FindByUserIdAsync(It.IsAny<string>(), It.IsAny<Identifier>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(Optional<SSOUserRoot>.None);

            var result =
                await _service.SaveUserTokensAsync(_caller.Object, TestSSOAuthenticationProvider.Name, "auserid".ToId(),
                    tokens, CancellationToken.None);

            result.Should().BeError(ErrorCode.EntityNotFound);
            _repository.Verify(rep => rep.FindByUserIdAsync(TestSSOAuthenticationProvider.Name, "auserid".ToId(),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task WhenSaveUserTokensAsync_ThenSavesTokens()
        {
            _caller.Setup(cc => cc.CallerId)
                .Returns("auserid");
            var datum = DateTime.UtcNow;
            var tokens = new ProviderAuthenticationTokens
            {
                Provider = "aprovidername",
                AccessToken = new AuthenticationToken
                {
                    ExpiresOn = datum,
                    Type = TokenType.AccessToken,
                    Value = "anaccesstoken"
                },
                RefreshToken = new AuthenticationToken
                {
                    ExpiresOn = datum,
                    Type = TokenType.RefreshToken,
                    Value = "arefreshtoken"
                },
                OtherTokens =
                [
                    new AuthenticationToken
                    {
                        ExpiresOn = datum,
                        Type = TokenType.OtherToken,
                        Value = "anothertoken"
                    }
                ]
            };
            var ssoUser = SSOUserRoot.Create(_recorder.Object, _idFactory.Object,
                "aprovidername", "auserid".ToId()).Value;
            _repository.Setup(rep =>
                    rep.FindByUserIdAsync(It.IsAny<string>(), It.IsAny<Identifier>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(ssoUser.ToOptional());

            var result =
                await _service.SaveUserTokensAsync(_caller.Object, TestSSOAuthenticationProvider.Name, "auserid".ToId(),
                    tokens, CancellationToken.None);

            var expectedTokens = SSOAuthTokens.Create([
                SSOAuthToken.Create(SSOAuthTokenType.AccessToken, "anencryptedvalue", datum).Value,
                SSOAuthToken.Create(SSOAuthTokenType.RefreshToken, "anencryptedvalue", datum).Value,
                SSOAuthToken.Create(SSOAuthTokenType.OtherToken, "anencryptedvalue", datum).Value
            ]).Value;
            result.Should().BeSuccess();
            _repository.Verify(rep => rep.FindByUserIdAsync(TestSSOAuthenticationProvider.Name, "auserid".ToId(),
                It.IsAny<CancellationToken>()));
            _repository.Verify(rep => rep.SaveAsync(It.Is<SSOUserRoot>(user =>
                user.UserId == "auserid"
                && user.ProviderName == "aprovidername"
                && user.Tokens == expectedTokens
            ), It.IsAny<CancellationToken>()));
        }
        //
        // [Fact]
        // public async Task WhenGetTokensAsyncAndNotOwner_ThenReturnsError()
        // {
        //     var result = await _service.GetTokensAsync(_caller.Object, "auserid".ToId(), CancellationToken.None);
        //
        //     result.Should().BeError(ErrorCode.RoleViolation, IdentityDomain.Resources.SSOUserRoot_NotOwner);
        // }

        [Fact]
        public async Task WhenGetTokensAsyncAndNoTokens_ThenReturnsNone()
        {
            _caller.Setup(cc => cc.CallerId)
                .Returns("auserid");
            var ssoUser = SSOUserRoot.Create(_recorder.Object, _idFactory.Object,
                "aprovidername", "auserid".ToId()).Value;
            _repository.Setup(rep =>
                    rep.FindByUserIdAsync(It.IsAny<string>(), It.IsAny<Identifier>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(ssoUser.ToOptional());

            var result =
                await _service.GetTokensOnBehalfOfUserAsync(_caller.Object, "auserid".ToId(), CancellationToken.None);

            result.Should().BeSuccess();
            result.Value.Count.Should().Be(0);
            _repository.Verify(rep => rep.FindByUserIdAsync(TestSSOAuthenticationProvider.Name, "auserid".ToId(),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task WhenGetTokensAsync_ThenReturnsError()
        {
            _caller.Setup(cc => cc.CallerId)
                .Returns("auserid");
            var datum = DateTime.UtcNow;
            var ssoUser = SSOUserRoot.Create(_recorder.Object, _idFactory.Object,
                "aprovidername", "auserid".ToId()).Value;
            ssoUser.ChangeTokens("auserid".ToId(), SSOAuthTokens.Create([
                SSOAuthToken.Create(SSOAuthTokenType.AccessToken, "anencryptedvalue", datum).Value,
                SSOAuthToken.Create(SSOAuthTokenType.RefreshToken, "anencryptedvalue", datum).Value,
                SSOAuthToken.Create(SSOAuthTokenType.OtherToken, "anencryptedvalue", datum).Value
            ]).Value);
            _repository.Setup(rep =>
                    rep.FindByUserIdAsync(It.IsAny<string>(), It.IsAny<Identifier>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(ssoUser.ToOptional());

            var result =
                await _service.GetTokensOnBehalfOfUserAsync(_caller.Object, "auserid".ToId(), CancellationToken.None);

            result.Should().BeSuccess();
            result.Value.Count.Should().Be(1);
            result.Value[0].Provider.Should().Be(TestSSOAuthenticationProvider.Name);
            result.Value[0].AccessToken.ExpiresOn.Should().Be(datum);
            result.Value[0].AccessToken.Type.Should().Be(TokenType.AccessToken);
            result.Value[0].AccessToken.Value.Should().Be("adecryptedvalue");
            result.Value[0].RefreshToken!.ExpiresOn.Should().Be(datum);
            result.Value[0].RefreshToken!.Type.Should().Be(TokenType.RefreshToken);
            result.Value[0].RefreshToken!.Value.Should().Be("adecryptedvalue");
            result.Value[0].OtherTokens.Count.Should().Be(1);
            result.Value[0].OtherTokens[0].Type.Should().Be(TokenType.OtherToken);
            result.Value[0].OtherTokens[0].ExpiresOn.Should().Be(datum);
            result.Value[0].OtherTokens[0].Value.Should().Be("adecryptedvalue");
            _encryptionService.Verify(es => es.Decrypt("anencryptedvalue"), Times.Exactly(3));
            _repository.Verify(rep => rep.FindByUserIdAsync(TestSSOAuthenticationProvider.Name, "auserid".ToId(),
                It.IsAny<CancellationToken>()));
        }
    }
}

public class TestSSOAuthenticationProvider : ISSOAuthenticationProvider
{
    public const string Name = "atestprovider";

    public Task<Result<SSOUserInfo, Error>> AuthenticateAsync(ICallerContext caller, string authCode,
        string? emailAddress,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public string ProviderName => Name;

    public Task<Result<ProviderAuthenticationTokens, Error>> RefreshTokenAsync(ICallerContext caller,
        string refreshToken, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}