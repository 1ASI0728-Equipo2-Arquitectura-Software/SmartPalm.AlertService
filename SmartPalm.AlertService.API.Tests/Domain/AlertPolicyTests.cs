using SmartPalm.AlertService.Domain.Model.ValueObjects;
using SmartPalm.AlertService.Domain.Services;

namespace SmartPalm.AlertService.API.Tests.Domain;

public class AlertPolicyTests
{
    private readonly AlertClassificationPolicy _policy = new();

    [Theory]
    [InlineData(30.5, AlertLevel.Informational)]
    [InlineData(34.0, AlertLevel.Warning)]
    [InlineData(45.0, AlertLevel.Critical)]
    public void Classify_UsesDeviationFromExceededBoundary(double value, AlertLevel expected) =>
        Assert.Equal(expected, _policy.Classify(value, 20, 30));

    [Fact]
    public void DuplicateWindow_IsThirtyMinutes() => Assert.Equal(TimeSpan.FromMinutes(30), DuplicateAlertPolicy.Window);
}
