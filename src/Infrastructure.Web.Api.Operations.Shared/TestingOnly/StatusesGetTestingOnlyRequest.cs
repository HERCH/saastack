#if TESTINGONLY
using Infrastructure.Web.Api.Interfaces;

namespace Infrastructure.Web.Api.Operations.Shared.TestingOnly;

[Route("/testingonly/statuses/get", OperationMethod.Get, isTestingOnly: true)]
public class StatusesGetTestingOnlyRequest : IWebRequest<StatusesTestingOnlyResponse>;
#endif