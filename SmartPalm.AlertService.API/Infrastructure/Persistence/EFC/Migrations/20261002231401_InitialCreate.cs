using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartPalm.AlertService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "alert");

            migrationBuilder.CreateTable(
                name: "agronomist_affiliation_projections",
                schema: "alert",
                columns: table => new
                {
                    affiliation_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    agronomist_id = table.Column<long>(type: "bigint", nullable: false),
                    plantation_id = table.Column<long>(type: "bigint", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agronomist_affiliation_projections", x => x.affiliation_id);
                });

            migrationBuilder.CreateTable(
                name: "alerts",
                schema: "alert",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    source_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gateway_mac_address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    iot_device_mac_address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sensor_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    reading_value = table.Column<double>(type: "double precision", nullable: false),
                    threshold_min = table.Column<double>(type: "double precision", nullable: false),
                    threshold_max = table.Column<double>(type: "double precision", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    plantation_id = table.Column<long>(type: "bigint", nullable: true),
                    sector_id = table.Column<long>(type: "bigint", nullable: true),
                    message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    measured_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    acknowledged_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_alerts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "alert",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "alert",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plantation_projections",
                schema: "alert",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    palm_grower_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plantation_projections", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sector_projections",
                schema: "alert",
                columns: table => new
                {
                    sector_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    plantation_id = table.Column<long>(type: "bigint", nullable: false),
                    iot_device_mac_address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sector_projections", x => x.sector_id);
                });

            migrationBuilder.CreateTable(
                name: "user_alert_settings",
                schema: "alert",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    sensor_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    is_muted = table.Column<bool>(type: "boolean", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_alert_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notification_deliveries",
                schema: "alert",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    alert_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    attempted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    provider_message_id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_deliveries", x => x.id);
                    table.ForeignKey(
                        name: "fk_notification_deliveries_alerts_alert_id",
                        column: x => x.alert_id,
                        principalSchema: "alert",
                        principalTable: "alerts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_agronomist_affiliation_projections_agronomist_id_plantation",
                schema: "alert",
                table: "agronomist_affiliation_projections",
                columns: new[] { "agronomist_id", "plantation_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_alerts_iot_device_mac_address_sensor_type_created_at_utc",
                schema: "alert",
                table: "alerts",
                columns: new[] { "iot_device_mac_address", "sensor_type", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_alerts_plantation_id_status",
                schema: "alert",
                table: "alerts",
                columns: new[] { "plantation_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_alerts_source_event_id",
                schema: "alert",
                table: "alerts",
                column: "source_event_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_alerts_user_id_created_at_utc",
                schema: "alert",
                table: "alerts",
                columns: new[] { "user_id", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_notification_deliveries_alert_id",
                schema: "alert",
                table: "notification_deliveries",
                column: "alert_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_deliveries_status_created_at_utc",
                schema: "alert",
                table: "notification_deliveries",
                columns: new[] { "status", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_on_utc_occurred_on_utc",
                schema: "alert",
                table: "outbox_messages",
                columns: new[] { "processed_on_utc", "occurred_on_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_plantation_projections_palm_grower_id",
                schema: "alert",
                table: "plantation_projections",
                column: "palm_grower_id");

            migrationBuilder.CreateIndex(
                name: "ix_sector_projections_iot_device_mac_address",
                schema: "alert",
                table: "sector_projections",
                column: "iot_device_mac_address",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_alert_settings_user_id_sensor_type",
                schema: "alert",
                table: "user_alert_settings",
                columns: new[] { "user_id", "sensor_type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agronomist_affiliation_projections",
                schema: "alert");

            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "alert");

            migrationBuilder.DropTable(
                name: "notification_deliveries",
                schema: "alert");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "alert");

            migrationBuilder.DropTable(
                name: "plantation_projections",
                schema: "alert");

            migrationBuilder.DropTable(
                name: "sector_projections",
                schema: "alert");

            migrationBuilder.DropTable(
                name: "user_alert_settings",
                schema: "alert");

            migrationBuilder.DropTable(
                name: "alerts",
                schema: "alert");
        }
    }
}
