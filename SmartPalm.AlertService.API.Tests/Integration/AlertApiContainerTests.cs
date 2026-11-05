using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using RabbitMQ.Client;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace SmartPalm.AlertService.API.Tests.Integration;

public class AlertApiContainerTests : IAsyncLifetime
{
    private const string JwtKey = "alert-tests-signing-key-with-at-least-32-characters";
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("smartpalm_alert").WithUsername("smartpalm_alert").WithPassword("alert-tests").Build();
    private readonly RabbitMqContainer _rabbit = new RabbitMqBuilder("rabbitmq:4-management-alpine").Build();

    public async Task InitializeAsync() => await Task.WhenAll(_postgres.StartAsync(), _rabbit.StartAsync());
    public async Task DisposeAsync() => await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _rabbit.DisposeAsync().AsTask());

    [DockerRequiredFact, Trait("Category", "Integration")]
    public async Task CropProjectionsAndRepeatedThreshold_CreateExactlyOneAlert()
    {
        using var factory = CreateFactory(); using var client = factory.CreateClient();
        await using var connection = await new ConnectionFactory { Uri = new Uri(_rabbit.GetConnectionString()) }.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync("smartpalm.events", ExchangeType.Topic, true, false);

        await PublishUntilAsync(channel, "crop.plantation-created.v1", Guid.NewGuid(), new
        { plantationId = 77L, palmGrowerId = 42L, name = "Test Plantation", hectares = 10m, occurredAtUtc = DateTime.UtcNow },
            "SELECT COUNT(*) FROM alert.plantation_projections", 1);
        await PublishUntilAsync(channel, "crop.sector-assigned.v1", Guid.NewGuid(), new
        { sectorId = 9L, plantationId = 77L, deviceMacAddress = "DEVICE-01", name = "North", occurredAtUtc = DateTime.UtcNow },
            "SELECT COUNT(*) FROM alert.sector_projections", 1);

        var threshold = new { batchId = Guid.NewGuid(), gatewayMac = "EDGE-01", deviceMac = "DEVICE-01", sensorType = "Temperature", value = 45.0, min = 20.0, max = 30.0, measuredAt = DateTime.UtcNow };
        await PublishUntilAsync(channel, "ingestion.threshold-exceeded.v1", Guid.NewGuid(), threshold,
            "SELECT COUNT(*) FROM alert.alerts", 1);
        await PublishUntilAsync(channel, "ingestion.threshold-exceeded.v1", Guid.NewGuid(), threshold,
            "SELECT COUNT(*) FROM alert.outbox_messages WHERE event_type = 'alert.suppressed.v1'", 1);
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM alert.alerts"));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM alert.notification_deliveries"));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(42, "PalmGrower"));
        var response = await client.GetAsync("/api/v1/alerts");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var alertId = json.RootElement.EnumerateArray().Single().GetProperty("id").GetInt64();
        Assert.Equal(HttpStatusCode.NoContent, (await client.PatchAsync($"/api/v1/alerts/{alertId}", null)).StatusCode);
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM alert.outbox_messages WHERE event_type = 'alert.acknowledged.v1'"));
    }

    [DockerRequiredFact, Trait("Category", "Integration")]
    public async Task Alerts_WithoutJwt_ReturnUnauthorized()
    {
        using var factory = CreateFactory(); using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/alerts")).StatusCode);
    }

    private WebApplicationFactory<Program> CreateFactory() => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
    {
        builder.UseEnvironment("Testing"); builder.UseSetting("ConnectionStrings:AlertDatabase", _postgres.GetConnectionString());
        builder.UseSetting("Jwt:Issuer", "smartpalm.identity.tests"); builder.UseSetting("Jwt:Audience", "smartpalm.clients.tests"); builder.UseSetting("Jwt:Key", JwtKey);
        builder.UseSetting("RabbitMq:Enabled", "true"); builder.UseSetting("RabbitMq:Uri", _rabbit.GetConnectionString()); builder.UseSetting("RabbitMq:Queue", $"smartpalm.alert.tests.{Guid.NewGuid():N}");
        builder.UseSetting("Firebase:Enabled", "false");
    });

    private async Task PublishUntilAsync(IChannel channel, string eventType, Guid messageId, object payload, string verificationSql, long expected)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        for (var attempt = 0; attempt < 15; attempt++)
        {
            var properties = new BasicProperties { Persistent = true, MessageId = messageId.ToString(), Type = eventType, ContentType = "application/json" };
            await channel.BasicPublishAsync("smartpalm.events", eventType, false, properties, body);
            await Task.Delay(600);
            if (await ScalarAsync(verificationSql) >= expected) return;
        }
        Assert.Fail($"Event {eventType} was not processed within the retry window.");
    }

    private async Task<long> ScalarAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString()); await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection); return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    private static string CreateToken(long userId, string role)
    {
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken("smartpalm.identity.tests", "smartpalm.clients.tests",
            [new Claim("sub", userId.ToString()), new Claim("sid", userId.ToString()), new Claim("name", "integration-user"), new Claim("role", role)],
            expires: DateTime.UtcNow.AddMinutes(10), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
