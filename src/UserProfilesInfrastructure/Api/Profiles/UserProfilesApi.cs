using Application.Resources.Shared;
using Infrastructure.Interfaces;
using Infrastructure.Web.Api.Common.Extensions;
using Infrastructure.Web.Api.Interfaces;
using Infrastructure.Web.Api.Operations.Shared.UserProfiles;
using Microsoft.AspNetCore.Http;
using UserProfilesApplication;
using UserProfilesDomain;

namespace UserProfilesInfrastructure.Api.Profiles;

public class UserProfilesApi : IWebApiService
{
    private readonly ICallerContextFactory _contextFactory;
    private readonly IFileUploadService _fileUploadService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserProfilesApplication _userProfilesApplication;

    public UserProfilesApi(IHttpContextAccessor httpContextAccessor, IFileUploadService fileUploadService,
        ICallerContextFactory contextFactory, IUserProfilesApplication userProfilesApplication)
    {
        _httpContextAccessor = httpContextAccessor;
        _fileUploadService = fileUploadService;
        _contextFactory = contextFactory;
        _userProfilesApplication = userProfilesApplication;
    }

    public async Task<ApiResult<UserProfile, GetProfileResponse>> ChangeContactAddress(
        ChangeProfileContactAddressRequest request, CancellationToken cancellationToken)
    {
        var profile =
            await _userProfilesApplication.ChangeContactAddressAsync(_contextFactory.Create(), request.UserId,
                request.Line1, request.Line2, request.Line3, request.City, request.State, request.CountryCode,
                request.Zip,
                cancellationToken);

        return () =>
            profile.HandleApplicationResult<GetProfileResponse, UserProfile>(pro => new GetProfileResponse
                { Profile = pro });
    }

    public async Task<ApiResult<UserProfile, GetProfileResponse>> ChangeProfile(
        ChangeProfileRequest request, CancellationToken cancellationToken)
    {
        var profile =
            await _userProfilesApplication.ChangeProfileAsync(_contextFactory.Create(), request.UserId,
                request.FirstName, request.LastName, request.DisplayName, request.PhoneNumber, request.Timezone,
                cancellationToken);

        return () =>
            profile.HandleApplicationResult<GetProfileResponse, UserProfile>(pro => new GetProfileResponse
                { Profile = pro });
    }

    public async Task<ApiPutPatchResult<UserProfile, ChangeProfileAvatarResponse>> ChangeProfileAvatar(
        ChangeProfileAvatarRequest request, CancellationToken cancellationToken)
    {
        var httpRequest = _httpContextAccessor.HttpContext!.Request;
        var uploaded = httpRequest.GetUploadedFile(_fileUploadService, Validations.Avatar.MaxSizeInBytes,
            Validations.Avatar.AllowableContentTypes);
        if (!uploaded.IsSuccessful)
        {
            return () => uploaded.Error;
        }

        var profile =
            await _userProfilesApplication.ChangeProfileAvatarAsync(_contextFactory.Create(), request.UserId,
                uploaded.Value, cancellationToken);

        return () =>
            profile.HandleApplicationResult<ChangeProfileAvatarResponse, UserProfile>(pro =>
                new ChangeProfileAvatarResponse { Profile = pro });
    }

    public async Task<ApiResult<UserProfile, DeleteProfileAvatarResponse>> DeleteProfileAvatar(
        DeleteProfileAvatarRequest request, CancellationToken cancellationToken)
    {
        var profile =
            await _userProfilesApplication.DeleteProfileAvatarAsync(_contextFactory.Create(), request.UserId,
                cancellationToken);

        return () =>
            profile.HandleApplicationResult<DeleteProfileAvatarResponse, UserProfile>(pro =>
                new DeleteProfileAvatarResponse { Profile = pro });
    }
}