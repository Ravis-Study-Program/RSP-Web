using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSPWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class MockInterviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BehaviouralMockInterviewRound",
                columns: table => new
                {
                    BehaviouralMockInterviewRoundId = table.Column<Guid>(type: "uuid", nullable: false),
                    MockInterviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    BehavioralScore = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BehaviouralMockInterviewRound", x => x.BehaviouralMockInterviewRoundId);
                });

            migrationBuilder.CreateTable(
                name: "CustomMockInterviewRound",
                columns: table => new
                {
                    CustomMockInterviewRoundId = table.Column<Guid>(type: "uuid", nullable: false),
                    MockInterviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Link = table.Column<string>(type: "text", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomMockInterviewRound", x => x.CustomMockInterviewRoundId);
                });

            migrationBuilder.CreateTable(
                name: "LeetcodeMockInterviewRound",
                columns: table => new
                {
                    LeetcodeMockInterviewRoundId = table.Column<Guid>(type: "uuid", nullable: false),
                    MockInterviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfirmQuestionScore = table.Column<int>(type: "integer", nullable: false),
                    AlgorithmDesignScore = table.Column<int>(type: "integer", nullable: false),
                    ComplexityAnalysisScore = table.Column<int>(type: "integer", nullable: false),
                    CodingScore = table.Column<int>(type: "integer", nullable: false),
                    TestingScore = table.Column<int>(type: "integer", nullable: false),
                    LeetcodeProblemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeetcodeMockInterviewRound", x => x.LeetcodeMockInterviewRoundId);
                    table.ForeignKey(
                        name: "FK_LeetcodeMockInterviewRound_LeetcodeProblems_LeetcodeProblem~",
                        column: x => x.LeetcodeProblemId,
                        principalTable: "LeetcodeProblems",
                        principalColumn: "LeetcodeProblemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MockInterviews",
                columns: table => new
                {
                    MockInterviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPass = table.Column<bool>(type: "boolean", nullable: false),
                    InterviewerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntervieweeUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimeTakenInMinutes = table.Column<int>(type: "integer", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockInterviews", x => x.MockInterviewId);
                    table.ForeignKey(
                        name: "FK_MockInterviews_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "EnrollmentId");
                    table.ForeignKey(
                        name: "FK_MockInterviews_Users_IntervieweeUserId",
                        column: x => x.IntervieweeUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MockInterviews_Users_InterviewerUserId",
                        column: x => x.InterviewerUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MockInterviewRound",
                columns: table => new
                {
                    MockInterviewRoundId = table.Column<Guid>(type: "uuid", nullable: false),
                    MockInterviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsReviewedByInterviewee = table.Column<bool>(type: "boolean", nullable: false),
                    IntervieweeComment = table.Column<string>(type: "text", nullable: false),
                    BehaviouralMockInterviewRoundId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeetcodeMockInterviewRoundId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomMockInterviewRoundId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockInterviewRound", x => x.MockInterviewRoundId);
                    table.ForeignKey(
                        name: "FK_MockInterviewRound_BehaviouralMockInterviewRound_Behavioura~",
                        column: x => x.BehaviouralMockInterviewRoundId,
                        principalTable: "BehaviouralMockInterviewRound",
                        principalColumn: "BehaviouralMockInterviewRoundId");
                    table.ForeignKey(
                        name: "FK_MockInterviewRound_CustomMockInterviewRound_CustomMockInter~",
                        column: x => x.CustomMockInterviewRoundId,
                        principalTable: "CustomMockInterviewRound",
                        principalColumn: "CustomMockInterviewRoundId");
                    table.ForeignKey(
                        name: "FK_MockInterviewRound_LeetcodeMockInterviewRound_LeetcodeMockI~",
                        column: x => x.LeetcodeMockInterviewRoundId,
                        principalTable: "LeetcodeMockInterviewRound",
                        principalColumn: "LeetcodeMockInterviewRoundId");
                    table.ForeignKey(
                        name: "FK_MockInterviewRound_MockInterviews_MockInterviewId",
                        column: x => x.MockInterviewId,
                        principalTable: "MockInterviews",
                        principalColumn: "MockInterviewId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeetcodeMockInterviewRound_LeetcodeProblemId",
                table: "LeetcodeMockInterviewRound",
                column: "LeetcodeProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_MockInterviewRound_BehaviouralMockInterviewRoundId",
                table: "MockInterviewRound",
                column: "BehaviouralMockInterviewRoundId");

            migrationBuilder.CreateIndex(
                name: "IX_MockInterviewRound_CustomMockInterviewRoundId",
                table: "MockInterviewRound",
                column: "CustomMockInterviewRoundId");

            migrationBuilder.CreateIndex(
                name: "IX_MockInterviewRound_LeetcodeMockInterviewRoundId",
                table: "MockInterviewRound",
                column: "LeetcodeMockInterviewRoundId");

            migrationBuilder.CreateIndex(
                name: "IX_MockInterviewRound_MockInterviewId",
                table: "MockInterviewRound",
                column: "MockInterviewId");

            migrationBuilder.CreateIndex(
                name: "IX_MockInterviews_EnrollmentId",
                table: "MockInterviews",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MockInterviews_IntervieweeUserId",
                table: "MockInterviews",
                column: "IntervieweeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MockInterviews_InterviewerUserId",
                table: "MockInterviews",
                column: "InterviewerUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MockInterviewRound");

            migrationBuilder.DropTable(
                name: "BehaviouralMockInterviewRound");

            migrationBuilder.DropTable(
                name: "CustomMockInterviewRound");

            migrationBuilder.DropTable(
                name: "LeetcodeMockInterviewRound");

            migrationBuilder.DropTable(
                name: "MockInterviews");
        }
    }
}
