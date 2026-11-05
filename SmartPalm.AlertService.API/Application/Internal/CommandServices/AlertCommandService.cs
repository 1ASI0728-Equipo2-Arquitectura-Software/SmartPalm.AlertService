using SmartPalm.AlertService.Application.Contracts;
using SmartPalm.AlertService.Application.Ports;
using SmartPalm.AlertService.Domain.Model.Commands;
using SmartPalm.AlertService.Domain.Model.Entities;
using SmartPalm.AlertService.Domain.Model.Events;
using SmartPalm.AlertService.Domain.Repositories;

namespace SmartPalm.AlertService.Application.CommandServices;

public class AlertCommandService(IAlertRepository alerts, IUserAlertSettingRepository settings,
    IUnitOfWork unitOfWork, IIntegrationEventWriter events)
{
    public async Task AcknowledgeAsync(AcknowledgeAlertCommand command, CancellationToken cancellationToken)
    {
        var alert = await alerts.GetAsync(command.AlertId, cancellationToken)
            ?? throw new KeyNotFoundException($"Alert '{command.AlertId}' not found.");
        if (!await alerts.CanAccessAsync(alert, command.UserId, command.Role, cancellationToken))
            throw new UnauthorizedAccessException("The alert does not belong to the authenticated user.");
        alert.Acknowledge(command.OccurredAtUtc);
        events.Enqueue("alert.acknowledged.v1", new AlertAcknowledgedIntegrationEvent(alert.Id, command.UserId, command.OccurredAtUtc));
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserAlertSettingResponse> UpdateSettingAsync(UpdateUserAlertSettingCommand command, CancellationToken cancellationToken)
    {
        var setting = await settings.FindAsync(command.UserId, command.SensorType, cancellationToken);
        if (setting is null) { setting = new UserAlertSetting(command.UserId, command.SensorType, command.IsMuted); await settings.AddAsync(setting, cancellationToken); }
        else setting.Update(command.IsMuted);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new UserAlertSettingResponse(setting.SensorType.ToString(), setting.IsMuted);
    }
}
