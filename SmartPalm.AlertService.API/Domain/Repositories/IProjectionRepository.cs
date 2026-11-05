using SmartPalm.AlertService.Domain.Model.Entities;

namespace SmartPalm.AlertService.Domain.Repositories;

public interface IProjectionRepository
{
    Task<PlantationProjection?> GetPlantationAsync(long id, CancellationToken cancellationToken);
    Task<SectorProjection?> FindSectorByDeviceAsync(string deviceMac, CancellationToken cancellationToken);
    Task<SectorProjection?> GetSectorAsync(long id, CancellationToken cancellationToken);
    Task<AgronomistAffiliationProjection?> GetAffiliationAsync(long id, CancellationToken cancellationToken);
    Task<AgronomistAffiliationProjection?> FindAffiliationAsync(long agronomistId, long plantationId, CancellationToken cancellationToken);
    Task AddPlantationAsync(PlantationProjection projection, CancellationToken cancellationToken);
    Task AddSectorAsync(SectorProjection projection, CancellationToken cancellationToken);
    Task AddAffiliationAsync(AgronomistAffiliationProjection projection, CancellationToken cancellationToken);
}
