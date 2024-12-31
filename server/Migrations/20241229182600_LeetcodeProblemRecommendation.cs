using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSPWebAPI.Migrations
{
  /// <inheritdoc />
  public partial class LeetcodeProblemRecommendation : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: "LeetcodeProblemRecommendation",
        columns: table => new
        {
          LeetcodeProblemRecommendationId = table.Column<string>(
            type: "varchar(16)",
            nullable: false
          ),
          UserId = table.Column<string>(type: "varchar(16)", nullable: false),
          LeetcodeProblemId = table.Column<string>(type: "varchar(16)", nullable: false),
          ProblemAttemptId = table.Column<string>(type: "varchar(16)", nullable: true),
          DeletedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
        },
        constraints: table =>
        {
          table.PrimaryKey(
            "PK_LeetcodeProblemRecommendation",
            x => x.LeetcodeProblemRecommendationId
          );
          table.ForeignKey(
            name: "FK_LeetcodeProblemRecommendation_LeetcodeProblem_LeetcodeProbl~",
            column: x => x.LeetcodeProblemId,
            principalTable: "LeetcodeProblem",
            principalColumn: "LeetcodeProblemId",
            onDelete: ReferentialAction.Restrict
          );
          table.ForeignKey(
            name: "FK_LeetcodeProblemRecommendation_ProblemAttempt_ProblemAttempt~",
            column: x => x.ProblemAttemptId,
            principalTable: "ProblemAttempt",
            principalColumn: "ProblemAttemptId",
            onDelete: ReferentialAction.SetNull
          );
          table.ForeignKey(
            name: "FK_LeetcodeProblemRecommendation_User_UserId",
            column: x => x.UserId,
            principalTable: "User",
            principalColumn: "UserId",
            onDelete: ReferentialAction.Restrict
          );
        }
      );

      migrationBuilder.CreateIndex(
        name: "IX_LeetcodeProblemRecommendation_LeetcodeProblemId",
        table: "LeetcodeProblemRecommendation",
        column: "LeetcodeProblemId"
      );

      migrationBuilder.CreateIndex(
        name: "IX_LeetcodeProblemRecommendation_ProblemAttemptId",
        table: "LeetcodeProblemRecommendation",
        column: "ProblemAttemptId"
      );

      migrationBuilder.CreateIndex(
        name: "IX_LeetcodeProblemRecommendation_UserId",
        table: "LeetcodeProblemRecommendation",
        column: "UserId"
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(name: "LeetcodeProblemRecommendation");
    }
  }
}
