using SmartPalm.AlertService.Domain.Model.Entities;
using SmartPalm.AlertService.Domain.Model.ValueObjects;

namespace SmartPalm.AlertService.Domain.Repositories;

public interface IUserAlertSettingRepository
{
    Task<UserAlertSetting?> FindAsync(long userId, SensorType sensorType, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserAlertSetting>> ListAsync(long userId, CancellationToken cancellationToken);
    Task AddAsync(UserAlertSetting setting, CancellationToken cancellationToken);
}
