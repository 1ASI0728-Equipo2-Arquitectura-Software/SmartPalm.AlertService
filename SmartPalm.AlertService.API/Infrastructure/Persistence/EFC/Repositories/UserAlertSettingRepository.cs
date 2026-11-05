using Microsoft.EntityFrameworkCore;
using SmartPalm.AlertService.Domain.Model.Entities;
using SmartPalm.AlertService.Domain.Model.ValueObjects;
using SmartPalm.AlertService.Domain.Repositories;

namespace SmartPalm.AlertService.Infrastructure.Persistence;

internal class UserAlertSettingRepository(AlertDbContext context) : IUserAlertSettingRepository
{
    public Task<UserAlertSetting?> FindAsync(long userId, SensorType sensorType, CancellationToken token) =>
        context.UserAlertSettings.FirstOrDefaultAsync(x => x.UserId == userId && x.SensorType == sensorType, token);
    public async Task<IReadOnlyList<UserAlertSetting>> ListAsync(long userId, CancellationToken token) =>
        await context.UserAlertSettings.Where(x => x.UserId == userId).OrderBy(x => x.SensorType).ToArrayAsync(token);
    public async Task AddAsync(UserAlertSetting setting, CancellationToken token) => await context.UserAlertSettings.AddAsync(setting, token);
}
