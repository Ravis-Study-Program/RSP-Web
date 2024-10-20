using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSPWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class ProblemAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProblemAttempts",
                columns: table => new
                {
                    ProblemAttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimeTakenInMinutes = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeetcodeProblemId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomProblemId = table.Column<Guid>(type: "uuid", nullable: true),
                    EnrollmentId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProblemAttempts", x => x.ProblemAttemptId);
                    table.ForeignKey(
                        name: "FK_ProblemAttempts_CustomProblems_CustomProblemId",
                        column: x => x.CustomProblemId,
                        principalTable: "CustomProblems",
                        principalColumn: "CustomProblemId");
                    table.ForeignKey(
                        name: "FK_ProblemAttempts_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "EnrollmentId");
                    table.ForeignKey(
                        name: "FK_ProblemAttempts_LeetcodeProblems_LeetcodeProblemId",
                        column: x => x.LeetcodeProblemId,
                        principalTable: "LeetcodeProblems",
                        principalColumn: "LeetcodeProblemId");
                    table.ForeignKey(
                        name: "FK_ProblemAttempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProblemAttempts_CustomProblemId",
                table: "ProblemAttempts",
                column: "CustomProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProblemAttempts_EnrollmentId",
                table: "ProblemAttempts",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProblemAttempts_LeetcodeProblemId",
                table: "ProblemAttempts",
                column: "LeetcodeProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProblemAttempts_UserId",
                table: "ProblemAttempts",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProblemAttempts");
        }
    }
}
