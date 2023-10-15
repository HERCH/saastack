#if TESTINGONLY
using JetBrains.Annotations;

namespace Infrastructure.WebApi.Interfaces.Operations.TestingOnly;

[Route("/testingonly/statuses/post2", ServiceOperation.Post, true)]
[UsedImplicitly]
public class StatusesPostWithLocationTestingOnlyRequest : IWebRequest<StatusesTestingOnlyResponse>
{
}
#endif