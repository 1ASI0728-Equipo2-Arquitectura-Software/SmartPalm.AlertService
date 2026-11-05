using SmartPalm.AlertService.Domain.Model.Entities;
using SmartPalm.AlertService.Domain.Repositories;

namespace SmartPalm.AlertService.Infrastructure.Persistence;

internal class NotificationDeliveryRepository(AlertDbContext context) : INotificationDeliveryRepository
{
    public async Task AddAsync(NotificationDelivery delivery, CancellationToken token) => await context.NotificationDeliveries.AddAsync(delivery, token);
}
