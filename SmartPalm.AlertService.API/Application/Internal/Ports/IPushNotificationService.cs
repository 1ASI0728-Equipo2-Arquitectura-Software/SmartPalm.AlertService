using SmartPalm.AlertService.Application.Contracts;

namespace SmartPalm.AlertService.Application.Ports;

public interface IPushNotificationService
{
    Task<PushDispatchResult> SendAsync(PushNotification notification, CancellationToken cancellationToken);
}
