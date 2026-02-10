using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fairline.Infrastructure.Migrations.Modeling
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "modeling");

            migrationBuilder.CreateTable(
                name: "scenarios",
                schema: "modeling",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scenarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "scenario_comparisons",
                schema: "modeling",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScenarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Market = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Selection = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProviderOdds = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    ModeledOdds = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Edge = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scenario_comparisons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_scenario_comparisons_scenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalSchema: "modeling",
                        principalTable: "scenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_scenario_comparisons_EventKey_Market",
                schema: "modeling",
                table: "scenario_comparisons",
                columns: new[] { "EventKey", "Market" });

            migrationBuilder.CreateIndex(
                name: "IX_scenario_comparisons_ScenarioId",
                schema: "modeling",
                table: "scenario_comparisons",
                column: "ScenarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "scenario_comparisons",
                schema: "modeling");

            migrationBuilder.DropTable(
                name: "scenarios",
                schema: "modeling");
        }
    }
}
