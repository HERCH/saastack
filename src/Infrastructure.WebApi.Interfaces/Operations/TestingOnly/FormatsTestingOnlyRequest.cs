#if TESTINGONLY
using JetBrains.Annotations;

namespace Infrastructure.WebApi.Interfaces.Operations.TestingOnly;

[Route("/testingonly/formats/roundtrip", ServiceOperation.Post, true)]
[UsedImplicitly]
public class FormatsTestingOnlyRequest : IWebRequest<FormatsTestingOnlyResponse>
{
    public CustomDto? Custom { get; set; }

    public double? Double { get; set; }

    public CustomEnum? Enum { get; set; }

    public int? Integer { get; set; }

    public string? String { get; set; }

    public DateTime? Time { get; set; }
}

public class CustomDto
{
    public double? Double { get; set; }

    public CustomEnum? Enum { get; set; }

    public int? Integer { get; set; }

    public string? String { get; set; }

    public DateTime? Time { get; set; }
}

public enum CustomEnum
{
    One,
    TwentyOne,
    OneHundredAndOne
}
#endif