using Microsoft.EntityFrameworkCore;
using SmartPalm.AlertService.Domain.Model.Aggregates;
using SmartPalm.AlertService.Domain.Model.Entities;
using SmartPalm.AlertService.Domain.Repositories;

namespace SmartPalm.AlertService.Infrastructure.Persistence;

internal class ProjectionRepository(AlertDbContext context) : IProjectionRepository
{
    public Task<PlantationProjection?> GetPlantationAsync(long id, CancellationToken token) => context.PlantationProjections.FirstOrDefaultAsync(x => x.Id == id, token);
    public Task<SectorProjection?> FindSectorByDeviceAsync(string deviceMac, CancellationToken token) => context.SectorProjections.FirstOrDefaultAsync(x => x.IotDeviceMacAddress == Alert.Normalize(deviceMac) && x.IsActive, token);
    public Task<SectorProjection?> GetSectorAsync(long id, CancellationToken token) => context.SectorProjections.FirstOrDefaultAsync(x => x.SectorId == id, token);
    public Task<AgronomistAffiliationProjection?> GetAffiliationAsync(long id, CancellationToken token) => context.AgronomistAffiliations.FirstOrDefaultAsync(x => x.AffiliationId == id, token);
    public Task<AgronomistAffiliationProjection?> FindAffiliationAsync(long agronomistId, long plantationId, CancellationToken token) => context.AgronomistAffiliations.FirstOrDefaultAsync(x => x.AgronomistId == agronomistId && x.PlantationId == plantationId, token);
    public async Task AddPlantationAsync(PlantationProjection projection, CancellationToken token) => await context.PlantationProjections.AddAsync(projection, token);
    public async Task AddSectorAsync(SectorProjection projection, CancellationToken token) => await context.SectorProjections.AddAsync(projection, token);
    public async Task AddAffiliationAsync(AgronomistAffiliationProjection projection, CancellationToken token) => await context.AgronomistAffiliations.AddAsync(projection, token);
}
