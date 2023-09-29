using Common.Extensions;
using FluentAssertions;
using Xunit;

namespace Common.UnitTests.Extensions;

[Trait("Category", "Unit")]
public class StringExtensionsSpec
{
    [Fact]
    public void WhenHasValueAndNull_ThenReturnsFalse()
    {
        var result = ((string?)null).HasValue();

        result.Should().BeFalse();
    }

    [Fact]
    public void WhenHasValueAndEmpty_ThenReturnsFalse()
    {
        var result = string.Empty.HasValue();

        result.Should().BeFalse();
    }

    [Fact]
    public void WhenHasValueAndOnlyWhitespace_ThenReturnsFalse()
    {
        var result = " ".HasValue();

        result.Should().BeFalse();
    }

    [Fact]
    public void WhenHasValueAndHasValue_ThenReturnsTrue()
    {
        var result = "avalue".HasValue();

        result.Should().BeTrue();
    }

    [Fact]
    public void WhenToJsonAndNull_ThenReturnsNull()
    {
        var result = ((string)null!).ToJson();

        result.Should().BeNull();
    }

    [Fact]
    public void WhenToJsonAndDefaults_ThenReturnsJsonPrettyPrintedWithNoNullsInPascal()
    {
        var result = new
        {
            Property1 = "avalue",
            Property2 = (string)null!,
            Property3 = ""
        }.ToJson();

        result.Should().Be("""
                           {
                             "Property1": "avalue",
                             "Property3": ""
                           }
                           """);
    }

    [Fact]
    public void WhenToJsonAndCamelCase_ThenReturnsJsonPrettyPrintedWithNoNullsInCamel()
    {
        var result = new
        {
            Property1 = "avalue",
            Property2 = (string)null!,
            Property3 = ""
        }.ToJson(casing: StringExtensions.JsonCasing.Camel);

        result.Should().Be("""
                           {
                             "property1": "avalue",
                             "property3": ""
                           }
                           """);
    }

    [Fact]
    public void WhenToJsonAndNotPretty_ThenReturnsJsonWithNoNullsInPascal()
    {
        var result = new
        {
            Property1 = "avalue",
            Property2 = (string)null!,
            Property3 = ""
        }.ToJson(false);

        result.Should().Be("{\"Property1\":\"avalue\",\"Property3\":\"\"}");
    }

    [Fact]
    public void WhenToJsonAndIncludeNulls_ThenReturnsJsonPrettyPrintedWithNullsInPascal()
    {
        var result = new
        {
            Property1 = "avalue",
            Property2 = (string)null!,
            Property3 = ""
        }.ToJson(includeNulls: true);

        result.Should().Be("""
                           {
                             "Property1": "avalue",
                             "Property2": null,
                             "Property3": ""
                           }
                           """);
    }

    [Fact]
    public void WhenIsMatchWithNull_ThenReturnsFalse()
    {
        var result = ((string)null!).IsMatchWith("apattern");

        result.Should().BeFalse();
    }

    [Fact]
    public void WhenIsMatchWithEmpty_ThenReturnsFalse()
    {
        var result = string.Empty.IsMatchWith("apattern");

        result.Should().BeFalse();
    }

    [Fact]
    public void WhenIsMatchWithNullPattern_ThenReturnsFalse()
    {
        var result = "avalue".IsMatchWith(null!);

        result.Should().BeFalse();
    }

    [Fact]
    public void WhenIsMatchWithEmptyAndEmptyPattern_ThenReturnsTrue()
    {
        var result = string.Empty.IsMatchWith(string.Empty);

        result.Should().BeTrue();
    }

    [Fact]
    public void WhenIsMatchAndNotMatches_ThenReturnsFalse()
    {
        var result = "avalue".IsMatchWith("anothervalue");

        result.Should().BeFalse();
    }

    [Fact]
    public void WhenIsMatchAndMatches_ThenReturnsTrue()
    {
        var result = "avalue".IsMatchWith("avalue");

        result.Should().BeTrue();
    }

    [Fact]
    public void WhenReplaceWithNull_ThenReturnsNull()
    {
        var result = ((string)null!).ReplaceWith("apattern", "areplacement");

        result.Should().BeNull();
    }

    [Fact]
    public void WhenReplaceWithEmpty_ThenReturnsEmpty()
    {
        var result = string.Empty.ReplaceWith("apattern", "areplacement");

        result.Should().BeEmpty();
    }

    [Fact]
    public void WhenReplaceWithNullPattern_ThenReturnsInput()
    {
        var result = "avalue".ReplaceWith(null!, "areplacement");

        result.Should().Be("avalue");
    }

    [Fact]
    public void WhenReplaceWithEmptyAndEmptyPattern_ThenReturnsInput()
    {
        var result = "avalue".ReplaceWith(string.Empty, "areplacement");

        result.Should().Be("avalue");
    }

    [Fact]
    public void WhenReplaceWithNoMatches_ThenReturnsInput()
    {
        var result = "avalue".ReplaceWith("apattern", "areplacement");

        result.Should().Be("avalue");
    }

    [Fact]
    public void WhenReplaceWithMatches_ThenReturnsReplaced()
    {
        var result = "avalue".ReplaceWith("a", "b");

        result.Should().Be("bvblue");
    }

    [Fact]
    public void WhenToBoolAndEmpty_ThenReturnsFalse()
    {
        var result = "".ToBool();

        result.Should().BeFalse();
    }

    [Fact]
    public void WhenToBoolAndNotMatches_ThenThrows()
    {
        "notavalue".Invoking(x => x.ToBool()).Should().Throw<FormatException>();
    }

    [Fact]
    public void WhenToBoolAndMatchesLowercase_ThenReturnsTrue()
    {
        var result = "true".ToBool();

        result.Should().BeTrue();
    }

    [Fact]
    public void WhenToBoolAndMatchesUppercase_ThenReturnsTrue()
    {
        var result = "TRUE".ToBool();

        result.Should().BeTrue();
    }

    [Fact]
    public void WhenToBoolAndMatchesMixedcase_ThenReturnsTrue()
    {
        var result = "True".ToBool();

        result.Should().BeTrue();
    }

    [Fact]
    public void WhenToBoolOrDefaultAndEmpty_ThenReturnsDefault()
    {
        var result = "".ToBoolOrDefault(true);

        result.Should().BeTrue();
    }

    [Fact]
    public void WhenToBoolOrDefaultAndNotMatches_ThenReturnsDefault()
    {
        var result = "notavalue".ToBoolOrDefault(true);

        result.Should().BeTrue();
    }

    [Fact]
    public void WhenToBoolOrDefaultAndMatchesLowercase_ThenReturnsMatched()
    {
        var result = "false".ToBoolOrDefault(true);

        result.Should().BeFalse();
    }

    [Fact]
    public void WhenToBoolOrDefaultAndMatchesUppercase_ThenReturnsMatched()
    {
        var result = "FALSE".ToBoolOrDefault(true);

        result.Should().BeFalse();
    }

    [Fact]
    public void WhenToBoolOrDefaultAndMatchesMixedcase_ThenReturnsMatched()
    {
        var result = "False".ToBoolOrDefault(true);

        result.Should().BeFalse();
    }

    [Fact]
    public void WhenToIntAndEmpty_ThenReturnsMinusOne()
    {
        var result = "".ToInt();

        result.Should().Be(-1);
    }

    [Fact]
    public void WhenToIntAndNotMatches_ThenThrows()
    {
        "notavalue".Invoking(x => x.ToInt()).Should().Throw<FormatException>();
    }

    [Fact]
    public void WhenToIntAndMatchesLowercase_ThenReturnsTrue()
    {
        var result = "9".ToInt();

        result.Should().Be(9);
    }

    [Fact]
    public void WhenToIntOrDefaultAndEmpty_ThenReturnsDefault()
    {
        var result = "".ToIntOrDefault(9);

        result.Should().Be(9);
    }

    [Fact]
    public void WhenToIntOrDefaultAndNotMatches_ThenReturnsDefault()
    {
        var result = "notavalue".ToIntOrDefault(9);

        result.Should().Be(9);
    }
}