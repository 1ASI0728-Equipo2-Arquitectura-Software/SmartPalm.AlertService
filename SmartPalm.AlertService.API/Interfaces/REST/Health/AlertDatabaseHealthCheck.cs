using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartPalm.AlertService.Infrastructure.Persistence;

namespace SmartPalm.AlertService.Api;

public class AlertDatabaseHealthCheck(AlertDbContext database) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext healthContext, CancellationToken token = default) =>
        await database.Database.CanConnectAsync(token) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("Alert database is unavailable.");
}
