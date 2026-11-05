namespace SmartPalm.AlertService.Domain.Model.Entities;

public class AgronomistAffiliationProjection
{
    private AgronomistAffiliationProjection() { }
    public AgronomistAffiliationProjection(long affiliationId, long agronomistId, long plantationId) { AffiliationId = affiliationId; AgronomistId = agronomistId; PlantationId = plantationId; IsActive = true; }
    public long AffiliationId { get; private set; }
    public long AgronomistId { get; private set; }
    public long PlantationId { get; private set; }
    public bool IsActive { get; private set; }
    public void Activate(long agronomistId, long plantationId) { AgronomistId = agronomistId; PlantationId = plantationId; IsActive = true; }
    public void Detach() => IsActive = false;
}
