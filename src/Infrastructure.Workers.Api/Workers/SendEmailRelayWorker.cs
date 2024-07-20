using Application.Interfaces.Services;
using Application.Persistence.Shared.ReadModels;
using Common;
using Common.Extensions;
using Infrastructure.Web.Api.Operations.Shared.Ancillary;
using Infrastructure.Web.Interfaces.Clients;
using Task = System.Threading.Tasks.Task;

namespace Infrastructure.Workers.Api.Workers;

public sealed class SendEmailRelayWorker : IQueueMonitoringApiRelayWorker<EmailMessage>
{
    private readonly IRecorder _recorder;
    private readonly IServiceClient _serviceClient;
    private readonly IHostSettings _settings;

    public SendEmailRelayWorker(IRecorder recorder, IHostSettings settings, IServiceClient serviceClient)
    {
        _recorder = recorder;
        _settings = settings;
        _serviceClient = serviceClient;
    }

    public async Task RelayMessageOrThrowAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        await _serviceClient.PostQueuedMessageToApiOrThrowAsync(_recorder,
            message, new SendEmailRequest
            {
                Message = message.ToJson()!
            }, _settings.GetAncillaryApiHostHmacAuthSecret(), cancellationToken);
    }
}