namespace SmartPalm.AlertService.Domain.Model.Entities;

public class PlantationProjection
{
    private PlantationProjection() { Name = string.Empty; }
    public PlantationProjection(long id, long palmGrowerId, string name) { Id = id; PalmGrowerId = palmGrowerId; Name = name; }
    public long Id { get; private set; }
    public long PalmGrowerId { get; private set; }
    public string Name { get; private set; }
    public void Refresh(long palmGrowerId, string name) { PalmGrowerId = palmGrowerId; Name = name; }
}
