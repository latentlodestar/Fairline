using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fairline.Infrastructure.Migrations.Ingest
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ingest");

            migrationBuilder.CreateTable(
                name: "providers",
                schema: "ingest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_providers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "odds_records",
                schema: "ingest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Market = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Selection = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Odds = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    RawPayload = table.Column<string>(type: "jsonb", nullable: true),
                    IngestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_odds_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_odds_records_providers_ProviderId",
                        column: x => x.ProviderId,
                        principalSchema: "ingest",
                        principalTable: "providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_odds_records_EventKey_Market",
                schema: "ingest",
                table: "odds_records",
                columns: new[] { "EventKey", "Market" });

            migrationBuilder.CreateIndex(
                name: "IX_odds_records_ProviderId",
                schema: "ingest",
                table: "odds_records",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_providers_Slug",
                schema: "ingest",
                table: "providers",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "odds_records",
                schema: "ingest");

            migrationBuilder.DropTable(
                name: "providers",
                schema: "ingest");
        }
    }
}
