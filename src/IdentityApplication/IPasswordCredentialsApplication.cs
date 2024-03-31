using Application.Interfaces;
using Application.Resources.Shared;
using Common;

namespace IdentityApplication;

public interface IPasswordCredentialsApplication
{
    Task<Result<AuthenticateTokens, Error>> AuthenticateAsync(ICallerContext context, string username, string password,
        CancellationToken cancellationToken);

    Task<Result<Error>> ConfirmPersonRegistrationAsync(ICallerContext context, string token,
        CancellationToken cancellationToken);

#if TESTINGONLY
    Task<Result<PasswordCredentialConfirmation, Error>> GetPersonRegistrationConfirmationAsync(ICallerContext context,
        string userId, CancellationToken cancellationToken);
#endif

    Task<Result<PasswordCredential, Error>> RegisterPersonAsync(ICallerContext context, string? invitationToken,
        string firstName,
        string lastName, string emailAddress, string password, string? timezone, string? countryCode,
        bool termsAndConditionsAccepted, CancellationToken cancellationToken);
}