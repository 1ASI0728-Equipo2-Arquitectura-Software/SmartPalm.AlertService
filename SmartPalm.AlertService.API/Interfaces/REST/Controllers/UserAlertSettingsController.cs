using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.AlertService.Application.CommandServices;
using SmartPalm.AlertService.Application.QueryServices;
using SmartPalm.AlertService.Domain.Model.Commands;
using SmartPalm.AlertService.Domain.Model.Queries;
using SmartPalm.AlertService.Domain.Model.ValueObjects;
using SmartPalm.AlertService.Interfaces.REST.Resources;
using SmartPalm.AlertService.Interfaces.REST.Transform;

namespace SmartPalm.AlertService.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/alert-settings")]
public class UserAlertSettingsController(AlertCommandService commandService, AlertQueryService queryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserAlertSettingResource>>> GetUserAlertSettings(CancellationToken token) => Ok(
        (await queryService.HandleAsync(new GetUserAlertSettingsQuery(User.RequiredUserId()), token)).Select(x => x.ToResource()));

    [HttpGet("{sensorType}")]
    public async Task<ActionResult<UserAlertSettingResource>> GetUserAlertSettingBySensorType(string sensorType, CancellationToken token)
    {
        if (!SensorTypeParser.TryParse(sensorType, out var parsed)) return BadRequest(new { message = $"Invalid sensor type: {sensorType}" });
        var result = await queryService.HandleAsync(new GetUserAlertSettingQuery(User.RequiredUserId(), parsed), token);
        return result is null ? NotFound(new { message = "Setting not found for this sensor type." }) : Ok(result.ToResource());
    }

    [HttpPut("{sensorType}")]
    public async Task<ActionResult<UserAlertSettingResource>> UpdateUserAlertSetting(string sensorType, UpdateUserAlertSettingResource resource, CancellationToken token)
    {
        if (!SensorTypeParser.TryParse(sensorType, out var parsed)) return BadRequest(new { message = $"Invalid sensor type: {sensorType}" });
        var result = await commandService.UpdateSettingAsync(new UpdateUserAlertSettingCommand(User.RequiredUserId(), parsed, resource.isMuted), token);
        return Ok(result.ToResource());
    }
}
