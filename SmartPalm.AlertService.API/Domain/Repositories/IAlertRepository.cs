using SmartPalm.AlertService.Domain.Model.Aggregates;
using SmartPalm.AlertService.Domain.Model.ValueObjects;

namespace SmartPalm.AlertService.Domain.Repositories;

public interface IAlertRepository
{
    Task AddAsync(Alert alert, CancellationToken cancellationToken);
    Task<Alert?> GetAsync(long id, CancellationToken cancellationToken);
    Task<Alert?> FindRecentAsync(string deviceMac, SensorType sensorType, DateTime sinceUtc, CancellationToken cancellationToken);
    Task<IReadOnlyList<Alert>> ListAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Alert>> ListForUserAsync(long userId, string role, CancellationToken cancellationToken);
    Task<bool> CanAccessAsync(Alert alert, long userId, string role, CancellationToken cancellationToken);
}
