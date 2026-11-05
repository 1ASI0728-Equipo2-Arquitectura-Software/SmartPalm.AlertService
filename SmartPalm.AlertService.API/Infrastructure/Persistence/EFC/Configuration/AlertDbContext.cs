using Microsoft.EntityFrameworkCore;
using SmartPalm.AlertService.Domain.Model.Aggregates;
using SmartPalm.AlertService.Domain.Model.Entities;
using SmartPalm.AlertService.Domain.Repositories;
using SmartPalm.AlertService.Infrastructure.Messaging;

namespace SmartPalm.AlertService.Infrastructure.Persistence;

public class AlertDbContext(DbContextOptions<AlertDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<UserAlertSetting> UserAlertSettings => Set<UserAlertSetting>();
    public DbSet<NotificationDelivery> NotificationDeliveries => Set<NotificationDelivery>();
    public DbSet<PlantationProjection> PlantationProjections => Set<PlantationProjection>();
    public DbSet<SectorProjection> SectorProjections => Set<SectorProjection>();
    public DbSet<AgronomistAffiliationProjection> AgronomistAffiliations => Set<AgronomistAffiliationProjection>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("alert");
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.ToTable("alerts"); entity.HasKey(x => x.Id); entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.GatewayMacAddress).HasMaxLength(100).IsRequired(); entity.Property(x => x.IotDeviceMacAddress).HasMaxLength(100).IsRequired();
            entity.Property(x => x.SensorType).HasConversion<string>().HasMaxLength(30); entity.Property(x => x.Level).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20); entity.Property(x => x.Message).HasMaxLength(500).IsRequired();
            entity.HasIndex(x => x.SourceEventId).IsUnique(); entity.HasIndex(x => new { x.IotDeviceMacAddress, x.SensorType, x.CreatedAtUtc });
            entity.HasIndex(x => new { x.UserId, x.CreatedAtUtc }); entity.HasIndex(x => new { x.PlantationId, x.Status });
        });
        modelBuilder.Entity<UserAlertSetting>(entity =>
        {
            entity.ToTable("user_alert_settings"); entity.HasKey(x => x.Id); entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.SensorType).HasConversion<string>().HasMaxLength(30); entity.HasIndex(x => new { x.UserId, x.SensorType }).IsUnique();
        });
        modelBuilder.Entity<NotificationDelivery>(entity =>
        {
            entity.ToTable("notification_deliveries"); entity.HasKey(x => x.Id); entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.ProviderMessageId).HasMaxLength(500); entity.Property(x => x.Error).HasMaxLength(2000);
            entity.HasOne<Alert>().WithMany().HasForeignKey(x => x.AlertId).OnDelete(DeleteBehavior.Cascade); entity.HasIndex(x => new { x.Status, x.CreatedAtUtc });
        });
        modelBuilder.Entity<PlantationProjection>(entity =>
        { entity.ToTable("plantation_projections"); entity.HasKey(x => x.Id); entity.Property(x => x.Name).HasMaxLength(200).IsRequired(); entity.HasIndex(x => x.PalmGrowerId); });
        modelBuilder.Entity<SectorProjection>(entity =>
        { entity.ToTable("sector_projections"); entity.HasKey(x => x.SectorId); entity.Property(x => x.IotDeviceMacAddress).HasMaxLength(100).IsRequired(); entity.HasIndex(x => x.IotDeviceMacAddress).IsUnique(); });
        modelBuilder.Entity<AgronomistAffiliationProjection>(entity =>
        { entity.ToTable("agronomist_affiliation_projections"); entity.HasKey(x => x.AffiliationId); entity.HasIndex(x => new { x.AgronomistId, x.PlantationId }).IsUnique(); });
        modelBuilder.Entity<InboxMessage>(entity =>
        { entity.ToTable("inbox_messages"); entity.HasKey(x => x.Id); entity.Property(x => x.EventType).HasMaxLength(200).IsRequired(); });
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox_messages"); entity.HasKey(x => x.Id); entity.Property(x => x.EventType).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Content).HasColumnType("jsonb").IsRequired(); entity.Property(x => x.Error).HasMaxLength(2000); entity.HasIndex(x => new { x.ProcessedOnUtc, x.OccurredOnUtc });
        });
    }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken token)
    {
        await using var transaction = await Database.BeginTransactionAsync(token);
        try { var result = await operation(token); await transaction.CommitAsync(token); return result; }
        catch { await transaction.RollbackAsync(token); throw; }
    }
}
