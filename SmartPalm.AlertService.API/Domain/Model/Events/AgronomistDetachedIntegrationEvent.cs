namespace SmartPalm.AlertService.Domain.Model.Events;

public record AgronomistDetachedIntegrationEvent(long AgronomistId, long PlantationId, DateTime OccurredAtUtc);
