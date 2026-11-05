namespace SmartPalm.AlertService.Application.Ports;

public interface IInbox
{
    Task<bool> HasProcessedAsync(Guid messageId, CancellationToken cancellationToken);
    void MarkProcessed(Guid messageId, string eventType);
}
