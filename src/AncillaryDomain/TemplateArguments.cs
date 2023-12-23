using Common;
using Common.Extensions;
using Domain.Common.ValueObjects;
using Domain.Interfaces;

namespace AncillaryDomain;

public class TemplateArguments : SingleValueObjectBase<TemplateArguments, List<string>>
{
    public static Result<TemplateArguments, Error> Create()
    {
        return Create(new List<string>());
    }

    public static Result<TemplateArguments, Error> Create(List<string> value)
    {
        return new TemplateArguments(value);
    }

    private TemplateArguments(List<string> arguments) : base(arguments)
    {
    }

    public List<string> Items => Value;

    public static ValueObjectFactory<TemplateArguments> Rehydrate()
    {
        return (property, _) =>
        {
            var items = RehydrateToList(property, true, true);
            return new TemplateArguments(
                items.Select(item => item)
                    .Where(item => item.Exists())
                    .ToList()!);
        };
    }
}