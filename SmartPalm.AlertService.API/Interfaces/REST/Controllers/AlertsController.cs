using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.AlertService.Application.CommandServices;
using SmartPalm.AlertService.Application.QueryServices;
using SmartPalm.AlertService.Domain.Model.Commands;
using SmartPalm.AlertService.Domain.Model.Queries;
using SmartPalm.AlertService.Interfaces.REST.Resources;
using SmartPalm.AlertService.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;

namespace SmartPalm.AlertService.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrator,PalmGrower,Agronomist")]
[Route("api/v1/alerts")]
[SwaggerTag("Alert & Notification endpoints")]
public class AlertsController(AlertCommandService commandService, AlertQueryService queryService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Get my alerts", OperationId = "GetMyAlerts")]
    public async Task<ActionResult<IReadOnlyList<AlertResource>>> GetAlerts(CancellationToken token) => Ok(
        (await queryService.HandleAsync(new GetAlertsByUserIdQuery(User.RequiredUserId(), User.RequiredRole()), token)).Select(x => x.ToResource()));

    [HttpPatch("{alertId:long}")]
    [SwaggerOperation(Summary = "Acknowledge an alert", OperationId = "AcknowledgeAlert")]
    public async Task<IActionResult> AcknowledgeAlert(long alertId, CancellationToken token)
    {
        await commandService.AcknowledgeAsync(new AcknowledgeAlertCommand(alertId, User.RequiredUserId(), User.RequiredRole(), DateTime.UtcNow), token);
        return NoContent();
    }
}
