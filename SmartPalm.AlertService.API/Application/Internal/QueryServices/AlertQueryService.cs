using SmartPalm.AlertService.Application.Contracts;
using SmartPalm.AlertService.Domain.Model.Aggregates;
using SmartPalm.AlertService.Domain.Model.Queries;
using SmartPalm.AlertService.Domain.Repositories;

namespace SmartPalm.AlertService.Application.QueryServices;

public class AlertQueryService(IAlertRepository alerts, IUserAlertSettingRepository settings)
{
    public async Task<IReadOnlyList<AlertResponse>> HandleAsync(GetAllAlertsQuery query, CancellationToken token) =>
        (await alerts.ListAllAsync(token)).Select(Map).ToArray();
    public async Task<IReadOnlyList<AlertResponse>> HandleAsync(GetAlertsByUserIdQuery query, CancellationToken token) =>
        (await alerts.ListForUserAsync(query.UserId, query.Role, token)).Select(Map).ToArray();
    public async Task<AlertResponse?> HandleAsync(GetAlertByIdQuery query, CancellationToken token)
    {
        var alert = await alerts.GetAsync(query.AlertId, token);
        return alert is not null && await alerts.CanAccessAsync(alert, query.UserId, query.Role, token) ? Map(alert) : null;
    }
    public async Task<IReadOnlyList<UserAlertSettingResponse>> HandleAsync(GetUserAlertSettingsQuery query, CancellationToken token) =>
        (await settings.ListAsync(query.UserId, token)).Select(x => new UserAlertSettingResponse(x.SensorType.ToString(), x.IsMuted)).ToArray();
    public async Task<UserAlertSettingResponse?> HandleAsync(GetUserAlertSettingQuery query, CancellationToken token)
    {
        var setting = await settings.FindAsync(query.UserId, query.SensorType, token);
        return setting is null ? null : new UserAlertSettingResponse(setting.SensorType.ToString(), setting.IsMuted);
    }
    private static AlertResponse Map(Alert value) => new(value.Id, value.SensorType.ToString(), value.Message,
        value.Level.ToString(), value.Status.ToString(), value.CreatedAtUtc.ToString("O"));
}
