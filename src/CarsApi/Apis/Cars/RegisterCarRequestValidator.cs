using CarsDomain;
using FluentValidation;
using Infrastructure.Web.Api.Common.Validation;
using Infrastructure.Web.Api.Interfaces.Operations.Cars;
using JetBrains.Annotations;

namespace CarsApi.Apis.Cars;

[UsedImplicitly]
public class RegisterCarRequestValidator : AbstractValidator<RegisterCarRequest>
{
    public RegisterCarRequestValidator()
    {
        RuleFor(req => req.Make)
            .Matches(Validations.Car.Make)
            .WithMessage(ValidationResources.RegisterCarRequestValidator_InvalidMake);
        RuleFor(req => req.Model)
            .Matches(Validations.Car.Model)
            .WithMessage(ValidationResources.RegisterCarRequestValidator_InvalidModel);
        RuleFor(req => req.Year)
            .InclusiveBetween(Validations.Car.Year.Min, Validations.Car.Year.Max)
            .WithMessage(ValidationResources.RegisterCarRequestValidator_InvalidYear);
        RuleFor(dto => dto.Jurisdiction)
            .Matches(Validations.Car.Jurisdiction)
            .Must(dto => Jurisdiction.AllowedCountries.Contains(dto))
            .WithMessage(ValidationResources.RegisterCarRequestValidator_InvalidJurisdiction);
        RuleFor(dto => dto.NumberPlate)
            .Matches(Validations.Car.NumberPlate)
            .WithMessage(ValidationResources.RegisterCarRequestValidator_InvalidNumberPlate);
    }
}