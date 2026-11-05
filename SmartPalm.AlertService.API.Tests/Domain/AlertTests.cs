using SmartPalm.AlertService.Domain.Model.Aggregates;
using SmartPalm.AlertService.Domain.Model.ValueObjects;

namespace SmartPalm.AlertService.API.Tests.Domain;

public class AlertTests
{
    [Fact]
    public void Constructor_NormalizesIdentifiersAndStartsActive()
    {
        var alert = Create();
        Assert.Equal("EDGE-01", alert.GatewayMacAddress);
        Assert.Equal("DEVICE-01", alert.IotDeviceMacAddress);
        Assert.Equal(AlertStatus.Active, alert.Status);
    }

    [Fact]
    public void Acknowledge_TransitionsOnlyOnce()
    {
        var alert = Create(); alert.Acknowledge(DateTime.UtcNow);
        Assert.Equal(AlertStatus.Acknowledged, alert.Status);
        Assert.Throws<InvalidOperationException>(() => alert.Acknowledge(DateTime.UtcNow));
    }

    [Fact]
    public void Constructor_RejectsInvalidThresholdRange() => Assert.Throws<ArgumentException>(() =>
        new Alert(Guid.NewGuid(), Guid.NewGuid(), "edge", "device", SensorType.Temperature,
            40, 50, 30, 1, 1, 1, "invalid", AlertLevel.Warning, DateTime.UtcNow));

    private static Alert Create() => new(Guid.NewGuid(), Guid.NewGuid(), "edge-01", "device-01",
        SensorType.Temperature, 40, 20, 30, 1, 2, 3, "Temperature exceeded.", AlertLevel.Warning, DateTime.UtcNow);
}
