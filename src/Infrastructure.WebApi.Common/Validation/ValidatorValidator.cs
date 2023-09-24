using Common.Extensions;
using Domain.Interfaces.Validations;
using FluentValidation;
using FluentValidation.Validators;

namespace Infrastructure.WebApi.Common.Validation;

/// <summary>
///     Validates a validation
/// </summary>
internal class ValidatorValidator<T, TProperty> : PropertyValidator<T, TProperty>
    where TProperty : notnull
{
    private readonly Validation<TProperty> _validation;

    public ValidatorValidator(Validation<TProperty> validation)
    {
        _validation = validation;
    }

    public override string Name => nameof(ValidatorValidator<T, TProperty>);

    public override bool IsValid(ValidationContext<T> context, TProperty value)
    {
        var propertyValue = value;
        if (propertyValue.NotExists())
        {
            return false;
        }

        return _validation.Matches(propertyValue);
    }

    protected override string GetDefaultMessageTemplate(string errorCode)
    {
        return Resources.ValidationValidator_InvalidProperty;
    }
}