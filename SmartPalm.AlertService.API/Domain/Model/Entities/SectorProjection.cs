using SmartPalm.AlertService.Domain.Model.Aggregates;

namespace SmartPalm.AlertService.Domain.Model.Entities;

public class SectorProjection
{
    private SectorProjection() { IotDeviceMacAddress = string.Empty; }
    public SectorProjection(long sectorId, long plantationId, string deviceMac) { SectorId = sectorId; PlantationId = plantationId; IotDeviceMacAddress = Alert.Normalize(deviceMac); IsActive = true; }
    public long SectorId { get; private set; }
    public long PlantationId { get; private set; }
    public string IotDeviceMacAddress { get; private set; }
    public bool IsActive { get; private set; }
    public void Assign(long plantationId, string deviceMac) { PlantationId = plantationId; IotDeviceMacAddress = Alert.Normalize(deviceMac); IsActive = true; }
    public void Remove() => IsActive = false;
}
