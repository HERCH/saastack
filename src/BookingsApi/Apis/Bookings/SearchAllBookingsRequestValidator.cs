using FluentValidation;
using Infrastructure.Web.Api.Common.Validation;
using Infrastructure.Web.Api.Interfaces.Operations.Bookings;
using JetBrains.Annotations;

namespace BookingsApi.Apis.Bookings;

[UsedImplicitly]
public class SearchAllBookingsRequestValidator : AbstractValidator<SearchAllBookingsRequest>
{
    public SearchAllBookingsRequestValidator(IHasSearchOptionsValidator hasSearchOptionsValidator)
    {
        Include(hasSearchOptionsValidator);

        RuleFor(req => req.ToUtc)
            .GreaterThan(req => req.FromUtc)
            .When(req => req.ToUtc.HasValue && req.FromUtc.HasValue)
            .WithMessage(ValidationResources.SearchAllBookingsRequestValidator_InvalidToUtc);
    }
}