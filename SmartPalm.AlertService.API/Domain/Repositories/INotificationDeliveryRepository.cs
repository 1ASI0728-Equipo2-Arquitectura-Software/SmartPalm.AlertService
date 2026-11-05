using SmartPalm.AlertService.Domain.Model.Entities;

namespace SmartPalm.AlertService.Domain.Repositories;

public interface INotificationDeliveryRepository
{
    Task AddAsync(NotificationDelivery delivery, CancellationToken cancellationToken);
}
