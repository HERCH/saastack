using AncillaryInfrastructure.Api.Smses;
using Common.Extensions;
using FluentAssertions;
using FluentValidation;
using Infrastructure.Web.Api.Operations.Shared.Ancillary;
using UnitTesting.Common.Validation;
using Xunit;

namespace AncillaryInfrastructure.UnitTests.Api.Smses;

[Trait("Category", "Unit")]
public class SearchSmsDeliveriesRequestValidatorSpec
{
    private readonly SearchSmsDeliveriesRequest _dto;
    private readonly SearchSmsDeliveriesRequestValidator _validator;

    public SearchSmsDeliveriesRequestValidatorSpec()
    {
        _validator = new SearchSmsDeliveriesRequestValidator();
        _dto = new SearchSmsDeliveriesRequest();
    }

    [Fact]
    public void WhenAllProperties_ThenSucceeds()
    {
        _validator.ValidateAndThrow(_dto);
    }

    [Fact]
    public void WhenSinceUtcIsTooFuture_ThenThrows()
    {
        _dto.SinceUtc = DateTime.UtcNow.AddHours(2);

        _validator.Invoking(x => x.ValidateAndThrow(_dto))
            .Should().Throw<ValidationException>()
            .WithMessageLike(Resources.SearchEmailDeliveriesRequestValidator_SinceUtc_TooFuture);
    }

    [Fact]
    public void WhenSinceUtcIsPast_ThenSucceeds()
    {
        _dto.SinceUtc = DateTime.UtcNow.SubtractSeconds(1);

        _validator.ValidateAndThrow(_dto);
    }

    [Fact]
    public void WhenTagIsInvalid_ThenThrows()
    {
        _dto.Tags = ["atag1,^aninvalidtag^"];

        _validator.Invoking(x => x.ValidateAndThrow(_dto))
            .Should().Throw<ValidationException>()
            .WithMessageLike(Resources.SearchEmailDeliveriesRequestValidator_InvalidTag);
    }

    [Fact]
    public void WhenTagIsValid_ThenSucceeds()
    {
        _dto.Tags = ["atag1"];

        _validator.ValidateAndThrow(_dto);
    }
}