using Microsoft.EntityFrameworkCore;
using SmartPalm.AlertService.Domain.Model.Aggregates;
using SmartPalm.AlertService.Domain.Model.ValueObjects;
using SmartPalm.AlertService.Domain.Repositories;

namespace SmartPalm.AlertService.Infrastructure.Persistence;

internal class AlertRepository(AlertDbContext context) : IAlertRepository
{
    public async Task AddAsync(Alert alert, CancellationToken token) => await context.Alerts.AddAsync(alert, token);
    public Task<Alert?> GetAsync(long id, CancellationToken token) => context.Alerts.FirstOrDefaultAsync(x => x.Id == id, token);
    public Task<Alert?> FindRecentAsync(string deviceMac, SensorType sensorType, DateTime sinceUtc, CancellationToken token) =>
        context.Alerts.Where(x => x.IotDeviceMacAddress == Alert.Normalize(deviceMac) && x.SensorType == sensorType && x.CreatedAtUtc >= sinceUtc)
            .OrderByDescending(x => x.CreatedAtUtc).FirstOrDefaultAsync(token);
    public async Task<IReadOnlyList<Alert>> ListAllAsync(CancellationToken token) =>
        await context.Alerts.OrderByDescending(x => x.CreatedAtUtc).ToArrayAsync(token);
    public async Task<IReadOnlyList<Alert>> ListForUserAsync(long userId, string role, CancellationToken token)
    {
        if (string.Equals(role, "Administrator", StringComparison.OrdinalIgnoreCase)) return await ListAllAsync(token);
        if (string.Equals(role, "Agronomist", StringComparison.OrdinalIgnoreCase))
        {
            var plantations = await context.AgronomistAffiliations.Where(x => x.AgronomistId == userId && x.IsActive).Select(x => x.PlantationId).ToArrayAsync(token);
            return await context.Alerts.Where(x => x.UserId == userId || (x.PlantationId.HasValue && plantations.Contains(x.PlantationId.Value)))
                .OrderByDescending(x => x.CreatedAtUtc).ToArrayAsync(token);
        }
        return await context.Alerts.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAtUtc).ToArrayAsync(token);
    }
    public async Task<bool> CanAccessAsync(Alert alert, long userId, string role, CancellationToken token)
    {
        if (string.Equals(role, "Administrator", StringComparison.OrdinalIgnoreCase) || alert.UserId == userId) return true;
        return string.Equals(role, "Agronomist", StringComparison.OrdinalIgnoreCase) && alert.PlantationId.HasValue &&
            await context.AgronomistAffiliations.AnyAsync(x => x.AgronomistId == userId && x.PlantationId == alert.PlantationId && x.IsActive, token);
    }
}
