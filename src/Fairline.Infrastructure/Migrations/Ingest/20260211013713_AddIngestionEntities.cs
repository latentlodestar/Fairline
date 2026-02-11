using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fairline.Infrastructure.Migrations.Ingest
{
    /// <inheritdoc />
    public partial class AddIngestionEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "events",
                schema: "ingest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderEventId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SportKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SportTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HomeTeam = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    AwayTeam = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CommenceTimeUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ingest_logs",
                schema: "ingest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IngestRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ingest_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ingest_runs",
                schema: "ingest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RunType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RequestCount = table.Column<int>(type: "integer", nullable: false),
                    EventCount = table.Column<int>(type: "integer", nullable: false),
                    SnapshotCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ingest_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "provider_catalog_snapshots",
                schema: "ingest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RawJson = table.Column<string>(type: "jsonb", nullable: false),
                    SportCount = table.Column<int>(type: "integer", nullable: false),
                    CapturedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_catalog_snapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "provider_requests",
                schema: "ingest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IngestRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    QuotaUsed = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_requests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sport_catalog",
                schema: "ingest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderSportKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Group = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    HasOutrights = table.Column<bool>(type: "boolean", nullable: false),
                    NormalizedSport = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NormalizedLeague = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CapturedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sport_catalog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tracked_leagues",
                schema: "ingest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProviderSportKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tracked_leagues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "odds_snapshots",
                schema: "ingest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SportEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookmakerKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BookmakerTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MarketKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OutcomeName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Point = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    ProviderLastUpdate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CapturedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_odds_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_odds_snapshots_events_SportEventId",
                        column: x => x.SportEventId,
                        principalSchema: "ingest",
                        principalTable: "events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_events_CommenceTimeUtc",
                schema: "ingest",
                table: "events",
                column: "CommenceTimeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_events_ProviderEventId",
                schema: "ingest",
                table: "events",
                column: "ProviderEventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_events_SportKey",
                schema: "ingest",
                table: "events",
                column: "SportKey");

            migrationBuilder.CreateIndex(
                name: "IX_ingest_logs_IngestRunId",
                schema: "ingest",
                table: "ingest_logs",
                column: "IngestRunId");

            migrationBuilder.CreateIndex(
                name: "IX_ingest_logs_IngestRunId_CreatedAtUtc",
                schema: "ingest",
                table: "ingest_logs",
                columns: new[] { "IngestRunId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ingest_runs_StartedAtUtc",
                schema: "ingest",
                table: "ingest_runs",
                column: "StartedAtUtc",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_odds_snapshots_CapturedAtUtc",
                schema: "ingest",
                table: "odds_snapshots",
                column: "CapturedAtUtc",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "ix_odds_snapshots_latest_by_key",
                schema: "ingest",
                table: "odds_snapshots",
                columns: new[] { "SportEventId", "MarketKey", "OutcomeName", "BookmakerKey", "CapturedAtUtc" },
                descending: new[] { false, false, false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_odds_snapshots_SportEventId",
                schema: "ingest",
                table: "odds_snapshots",
                column: "SportEventId");

            migrationBuilder.CreateIndex(
                name: "IX_provider_catalog_snapshots_CapturedAtUtc",
                schema: "ingest",
                table: "provider_catalog_snapshots",
                column: "CapturedAtUtc",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_provider_requests_IngestRunId",
                schema: "ingest",
                table: "provider_requests",
                column: "IngestRunId");

            migrationBuilder.CreateIndex(
                name: "IX_sport_catalog_NormalizedSport",
                schema: "ingest",
                table: "sport_catalog",
                column: "NormalizedSport");

            migrationBuilder.CreateIndex(
                name: "IX_sport_catalog_ProviderSportKey",
                schema: "ingest",
                table: "sport_catalog",
                column: "ProviderSportKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tracked_leagues_Enabled",
                schema: "ingest",
                table: "tracked_leagues",
                column: "Enabled");

            migrationBuilder.CreateIndex(
                name: "IX_tracked_leagues_Provider_ProviderSportKey",
                schema: "ingest",
                table: "tracked_leagues",
                columns: new[] { "Provider", "ProviderSportKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ingest_logs",
                schema: "ingest");

            migrationBuilder.DropTable(
                name: "ingest_runs",
                schema: "ingest");

            migrationBuilder.DropTable(
                name: "odds_snapshots",
                schema: "ingest");

            migrationBuilder.DropTable(
                name: "provider_catalog_snapshots",
                schema: "ingest");

            migrationBuilder.DropTable(
                name: "provider_requests",
                schema: "ingest");

            migrationBuilder.DropTable(
                name: "sport_catalog",
                schema: "ingest");

            migrationBuilder.DropTable(
                name: "tracked_leagues",
                schema: "ingest");

            migrationBuilder.DropTable(
                name: "events",
                schema: "ingest");
        }
    }
}
