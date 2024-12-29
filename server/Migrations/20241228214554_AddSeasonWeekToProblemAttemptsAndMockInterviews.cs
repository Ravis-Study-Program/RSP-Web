using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSPWebAPI.Migrations
{
  /// <inheritdoc />
  public partial class AddSeasonWeekToProblemAttemptsAndMockInterviews : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<string>(
        name: "SeasonWeekId",
        table: "ProblemAttempt",
        type: "varchar(16)",
        nullable: true
      );

      migrationBuilder.AddColumn<string>(
        name: "SeasonWeekId",
        table: "MockInterview",
        type: "varchar(16)",
        nullable: true
      );

      migrationBuilder.CreateIndex(
        name: "IX_ProblemAttempt_SeasonWeekId",
        table: "ProblemAttempt",
        column: "SeasonWeekId"
      );

      migrationBuilder.CreateIndex(
        name: "IX_MockInterview_SeasonWeekId",
        table: "MockInterview",
        column: "SeasonWeekId"
      );

      migrationBuilder.AddForeignKey(
        name: "FK_MockInterview_SeasonWeek_SeasonWeekId",
        table: "MockInterview",
        column: "SeasonWeekId",
        principalTable: "SeasonWeek",
        principalColumn: "SeasonWeekId",
        onDelete: ReferentialAction.SetNull
      );

      migrationBuilder.AddForeignKey(
        name: "FK_ProblemAttempt_SeasonWeek_SeasonWeekId",
        table: "ProblemAttempt",
        column: "SeasonWeekId",
        principalTable: "SeasonWeek",
        principalColumn: "SeasonWeekId",
        onDelete: ReferentialAction.SetNull
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
        name: "FK_MockInterview_SeasonWeek_SeasonWeekId",
        table: "MockInterview"
      );

      migrationBuilder.DropForeignKey(
        name: "FK_ProblemAttempt_SeasonWeek_SeasonWeekId",
        table: "ProblemAttempt"
      );

      migrationBuilder.DropIndex(name: "IX_ProblemAttempt_SeasonWeekId", table: "ProblemAttempt");

      migrationBuilder.DropIndex(name: "IX_MockInterview_SeasonWeekId", table: "MockInterview");

      migrationBuilder.DropColumn(name: "SeasonWeekId", table: "ProblemAttempt");

      migrationBuilder.DropColumn(name: "SeasonWeekId", table: "MockInterview");
    }
  }
}
