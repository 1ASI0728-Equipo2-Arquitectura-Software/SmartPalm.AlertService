using System.Text.Json;
using System.Text.Json.Serialization;
using SmartPalm.AlertService.Application.Ports;
using SmartPalm.AlertService.Infrastructure.Persistence;

namespace SmartPalm.AlertService.Infrastructure.Messaging;

internal class IntegrationEventWriter(AlertDbContext context) : IIntegrationEventWriter
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    public void Enqueue(string eventType, object payload) => context.OutboxMessages.Add(new OutboxMessage
    { Id = Guid.NewGuid(), EventType = eventType, Content = JsonSerializer.Serialize(payload, payload.GetType(), JsonOptions), OccurredOnUtc = DateTime.UtcNow });

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
