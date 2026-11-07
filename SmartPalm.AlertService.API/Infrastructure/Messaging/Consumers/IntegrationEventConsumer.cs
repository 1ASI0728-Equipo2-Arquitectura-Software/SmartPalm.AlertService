using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartPalm.AlertService.Application.EventHandlers;
using SmartPalm.AlertService.Domain.Model.Events;
using SmartPalm.AlertService.Infrastructure.Configuration;

namespace SmartPalm.AlertService.Infrastructure.Messaging;

internal class IntegrationEventConsumer(IServiceScopeFactory scopes, IOptions<RabbitMqOptions> options,
    ILogger<IntegrationEventConsumer> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    private readonly RabbitMqOptions _options = options.Value;
    private static readonly string[] RoutingKeys =
    [
        "ingestion.threshold-exceeded.v1", "crop.plantation-created.v1", "crop.sector-assigned.v1",
        "crop.sector-removed.v1", "crop.agronomist-affiliated.v1", "crop.agronomist-detached.v1",
        "smartpalm.identity.user-deletion-requested.v1"
    ];

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        if (!_options.Enabled) return;
        while (!token.IsCancellationRequested)
        {
            try { await ConsumeAsync(token); }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            catch (Exception ex) { logger.LogWarning(ex, "Integration consumer disconnected; retrying."); await Task.Delay(TimeSpan.FromSeconds(3), token); }
        }
    }

    private async Task ConsumeAsync(CancellationToken token)
    {
        var factory = new ConnectionFactory { Uri = new Uri(_options.Uri), AutomaticRecoveryEnabled = true };
        await using var connection = await factory.CreateConnectionAsync(token);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: token);
        await channel.ExchangeDeclareAsync(_options.Exchange, ExchangeType.Topic, true, false, cancellationToken: token);
        await channel.QueueDeclareAsync(_options.Queue, true, false, false, cancellationToken: token);
        foreach (var key in RoutingKeys) await channel.QueueBindAsync(_options.Queue, _options.Exchange, key, cancellationToken: token);
        await channel.BasicQosAsync(0, 10, false, token);
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var eventType = args.BasicProperties.Type ?? args.RoutingKey;
                var id = Guid.TryParse(args.BasicProperties.MessageId, out var parsed) ? parsed : Guid.NewGuid();
                var body = args.Body.ToArray();
                using var scope = scopes.CreateScope(); var handlers = scope.ServiceProvider.GetRequiredService<IntegrationEventHandlers>();
                switch (eventType)
                {
                    case "ingestion.threshold-exceeded.v1": await handlers.HandleAsync(id, Deserialize<ThresholdExceededIntegrationEvent>(body), token); break;
                    case "crop.plantation-created.v1": await handlers.HandleAsync(id, Deserialize<PlantationCreatedIntegrationEvent>(body), token); break;
                    case "crop.sector-assigned.v1": await handlers.HandleAsync(id, Deserialize<SectorAssignedIntegrationEvent>(body), token); break;
                    case "crop.sector-removed.v1": await handlers.HandleAsync(id, Deserialize<SectorRemovedIntegrationEvent>(body), token); break;
                    case "crop.agronomist-affiliated.v1": await handlers.HandleAsync(id, Deserialize<AgronomistAffiliatedIntegrationEvent>(body), token); break;
                    case "crop.agronomist-detached.v1": await handlers.HandleAsync(id, Deserialize<AgronomistDetachedIntegrationEvent>(body), token); break;
                    case "smartpalm.identity.user-deletion-requested.v1": await handlers.HandleAsync(
                        DeserializePayload<UserDeletionRequestedIntegrationEvent>(body), token); break;
                }
                await channel.BasicAckAsync(args.DeliveryTag, false, token);
            }
            catch (Exception ex) { logger.LogError(ex, "Integration event {RoutingKey} failed.", args.RoutingKey); await channel.BasicNackAsync(args.DeliveryTag, false, true, token); }
        };
        await channel.BasicConsumeAsync(_options.Queue, false, consumer, token);
        await Task.Delay(Timeout.InfiniteTimeSpan, token);
    }

    private static T Deserialize<T>(byte[] body) => JsonSerializer.Deserialize<T>(body, JsonOptions)
        ?? throw new JsonException($"Invalid {typeof(T).Name} payload.");

    private static T DeserializePayload<T>(byte[] body)
    {
        using var document = JsonDocument.Parse(body);
        var payload = document.RootElement.TryGetProperty("payload", out var value)
            ? value
            : document.RootElement;
        return payload.Deserialize<T>(JsonOptions)
            ?? throw new JsonException($"Invalid {typeof(T).Name} payload.");
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
