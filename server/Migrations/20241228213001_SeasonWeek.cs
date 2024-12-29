using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSPWebAPI.Migrations
{
  /// <inheritdoc />
  public partial class SeasonWeek : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: "SeasonWeek",
        columns: table => new
        {
          SeasonWeekId = table.Column<string>(type: "varchar(16)", nullable: false),
          SeasonId = table.Column<string>(type: "varchar(16)", nullable: false),
          WeekNumber = table.Column<int>(type: "integer", nullable: false),
          StartDate = table.Column<DateTime>(type: "timestamptz", nullable: false),
          EndDate = table.Column<DateTime>(type: "timestamptz", nullable: false),
          DeletedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_SeasonWeek", x => x.SeasonWeekId);
          table.ForeignKey(
            name: "FK_SeasonWeek_Season_SeasonId",
            column: x => x.SeasonId,
            principalTable: "Season",
            principalColumn: "SeasonId",
            onDelete: ReferentialAction.Restrict
          );
        }
      );

      migrationBuilder.CreateIndex(
        name: "IX_SeasonWeek_SeasonId",
        table: "SeasonWeek",
        column: "SeasonId"
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(name: "SeasonWeek");
    }
  }
}
