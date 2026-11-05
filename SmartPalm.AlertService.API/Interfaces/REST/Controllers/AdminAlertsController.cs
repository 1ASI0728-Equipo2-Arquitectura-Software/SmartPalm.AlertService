using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.AlertService.Application.QueryServices;
using SmartPalm.AlertService.Domain.Model.Queries;
using SmartPalm.AlertService.Interfaces.REST.Resources;
using SmartPalm.AlertService.Interfaces.REST.Transform;

namespace SmartPalm.AlertService.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("api/v1/admin/alerts")]
public class AdminAlertsController(AlertQueryService queryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AlertResource>>> ListAlerts(CancellationToken token) => Ok(
        (await queryService.HandleAsync(new GetAllAlertsQuery(), token)).Select(x => x.ToResource()));
}
