using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSPWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDatabaseIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeetcodeProblemCategory",
                columns: table => new
                {
                    LeetcodeProblemCategoryId = table.Column<string>(type: "varchar(32)", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeetcodeProblemCategory", x => x.LeetcodeProblemCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Problem",
                columns: table => new
                {
                    ProblemId = table.Column<string>(type: "varchar(16)", nullable: false),
                    Title = table.Column<string>(type: "varchar(100)", nullable: false),
                    Link = table.Column<string>(type: "varchar(255)", maxLength: 510, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Problem", x => x.ProblemId);
                });

            migrationBuilder.CreateTable(
                name: "Season",
                columns: table => new
                {
                    SeasonId = table.Column<string>(type: "varchar(16)", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    Slug = table.Column<string>(type: "varchar(32)", nullable: false),
                    StartDateInclusiveUTC = table.Column<DateTime>(type: "timestamp", nullable: false),
                    EndDateInclusiveUTC = table.Column<DateTime>(type: "timestamp", nullable: false),
                    Location = table.Column<string>(type: "varchar(100)", nullable: false),
                    ImageUrl = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Season", x => x.SeasonId);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(16)", nullable: false),
                    DiscordId = table.Column<string>(type: "varchar(16)", nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    ProfileImage = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "CustomProblem",
                columns: table => new
                {
                    CustomProblemId = table.Column<string>(type: "varchar(16)", nullable: false),
                    ProblemId = table.Column<string>(type: "varchar(16)", nullable: false),
                    Difficulty = table.Column<string>(type: "varchar(50)", nullable: false),
                    Question = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomProblem", x => x.CustomProblemId);
                    table.ForeignKey(
                        name: "FK_CustomProblem_Problem_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problem",
                        principalColumn: "ProblemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeetcodeProblem",
                columns: table => new
                {
                    LeetcodeProblemId = table.Column<string>(type: "varchar(16)", nullable: false),
                    ProblemId = table.Column<string>(type: "varchar(16)", nullable: false),
                    LeetcodeProblemDifficulty = table.Column<int>(type: "int", nullable: false),
                    IsPremium = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeetcodeProblem", x => x.LeetcodeProblemId);
                    table.ForeignKey(
                        name: "FK_LeetcodeProblem_Problem_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problem",
                        principalColumn: "ProblemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Enrollment",
                columns: table => new
                {
                    EnrollmentId = table.Column<string>(type: "varchar(16)", nullable: false),
                    SeasonId = table.Column<string>(type: "varchar(16)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(16)", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollment", x => x.EnrollmentId);
                    table.ForeignKey(
                        name: "FK_Enrollment_Season_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Season",
                        principalColumn: "SeasonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollment_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeetcodeProblemCategoryMapping",
                columns: table => new
                {
                    LeetcodeProblemCategoriesLeetcodeProblemCategoryId = table.Column<string>(type: "varchar(32)", nullable: false),
                    LeetcodeProblemEntityLeetcodeProblemId = table.Column<string>(type: "varchar(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeetcodeProblemCategoryMapping", x => new { x.LeetcodeProblemCategoriesLeetcodeProblemCategoryId, x.LeetcodeProblemEntityLeetcodeProblemId });
                    table.ForeignKey(
                        name: "FK_LeetcodeProblemCategoryMapping_LeetcodeProblemCategory_Leet~",
                        column: x => x.LeetcodeProblemCategoriesLeetcodeProblemCategoryId,
                        principalTable: "LeetcodeProblemCategory",
                        principalColumn: "LeetcodeProblemCategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeetcodeProblemCategoryMapping_LeetcodeProblem_LeetcodeProb~",
                        column: x => x.LeetcodeProblemEntityLeetcodeProblemId,
                        principalTable: "LeetcodeProblem",
                        principalColumn: "LeetcodeProblemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Mentorship",
                columns: table => new
                {
                    MentorshipId = table.Column<string>(type: "varchar(16)", nullable: false),
                    MentorEnrollmentId = table.Column<string>(type: "varchar(16)", nullable: false),
                    MenteeEnrollmentId = table.Column<string>(type: "varchar(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mentorship", x => x.MentorshipId);
                    table.ForeignKey(
                        name: "FK_Mentorship_Enrollment_MenteeEnrollmentId",
                        column: x => x.MenteeEnrollmentId,
                        principalTable: "Enrollment",
                        principalColumn: "EnrollmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Mentorship_Enrollment_MentorEnrollmentId",
                        column: x => x.MentorEnrollmentId,
                        principalTable: "Enrollment",
                        principalColumn: "EnrollmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MockInterview",
                columns: table => new
                {
                    MockInterviewId = table.Column<string>(type: "varchar(16)", maxLength: 32, nullable: false),
                    IsPass = table.Column<bool>(type: "boolean", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    TimeTakenInMinutes = table.Column<int>(type: "int", nullable: false),
                    InterviewerUserId = table.Column<string>(type: "varchar(16)", maxLength: 32, nullable: false),
                    IntervieweeUserId = table.Column<string>(type: "varchar(16)", maxLength: 32, nullable: false),
                    EnrollmentId = table.Column<string>(type: "varchar(16)", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockInterview", x => x.MockInterviewId);
                    table.ForeignKey(
                        name: "FK_MockInterview_Enrollment_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollment",
                        principalColumn: "EnrollmentId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MockInterview_User_IntervieweeUserId",
                        column: x => x.IntervieweeUserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MockInterview_User_InterviewerUserId",
                        column: x => x.InterviewerUserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProblemAttempt",
                columns: table => new
                {
                    ProblemAttemptId = table.Column<string>(type: "varchar(16)", maxLength: 32, nullable: false),
                    AttemptStartDateUtc = table.Column<DateTime>(type: "timestamp", nullable: false),
                    TimeTakenInMinutes = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "varchar(16)", maxLength: 10000, nullable: false),
                    UserId = table.Column<string>(type: "varchar(16)", maxLength: 32, nullable: false),
                    LeetcodeProblemId = table.Column<string>(type: "varchar(16)", maxLength: 32, nullable: true),
                    CustomProblemId = table.Column<string>(type: "varchar(16)", maxLength: 32, nullable: true),
                    EnrollmentId = table.Column<string>(type: "varchar(16)", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProblemAttempt", x => x.ProblemAttemptId);
                    table.ForeignKey(
                        name: "FK_ProblemAttempt_CustomProblem_CustomProblemId",
                        column: x => x.CustomProblemId,
                        principalTable: "CustomProblem",
                        principalColumn: "CustomProblemId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProblemAttempt_Enrollment_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollment",
                        principalColumn: "EnrollmentId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProblemAttempt_LeetcodeProblem_LeetcodeProblemId",
                        column: x => x.LeetcodeProblemId,
                        principalTable: "LeetcodeProblem",
                        principalColumn: "LeetcodeProblemId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProblemAttempt_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BehaviouralMockInterviewRound",
                columns: table => new
                {
                    BehaviouralMockInterviewRoundId = table.Column<string>(type: "varchar(32)", nullable: false),
                    MockInterviewId = table.Column<string>(type: "varchar(16)", nullable: false),
                    MockInterviewRoundId = table.Column<string>(type: "varchar(16)", nullable: false),
                    BehavioralScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BehaviouralMockInterviewRound", x => x.BehaviouralMockInterviewRoundId);
                    table.ForeignKey(
                        name: "FK_BehaviouralMockInterviewRound_MockInterview_MockInterviewId",
                        column: x => x.MockInterviewId,
                        principalTable: "MockInterview",
                        principalColumn: "MockInterviewId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomMockInterviewRound",
                columns: table => new
                {
                    CustomMockInterviewRoundId = table.Column<string>(type: "varchar(32)", nullable: false),
                    MockInterviewId = table.Column<string>(type: "varchar(16)", nullable: false),
                    MockInterviewRoundId = table.Column<string>(type: "varchar(16)", nullable: false),
                    Content = table.Column<string>(type: "varchar(10000)", nullable: false),
                    Link = table.Column<string>(type: "varchar(255)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomMockInterviewRound", x => x.CustomMockInterviewRoundId);
                    table.ForeignKey(
                        name: "FK_CustomMockInterviewRound_MockInterview_MockInterviewId",
                        column: x => x.MockInterviewId,
                        principalTable: "MockInterview",
                        principalColumn: "MockInterviewId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeetcodeMockInterviewRound",
                columns: table => new
                {
                    LeetcodeMockInterviewRoundId = table.Column<string>(type: "varchar(32)", nullable: false),
                    MockInterviewId = table.Column<string>(type: "varchar(16)", nullable: false),
                    MockInterviewRoundId = table.Column<string>(type: "varchar(16)", nullable: false),
                    LeetcodeProblemId = table.Column<string>(type: "varchar(16)", nullable: false),
                    ConfirmQuestionScore = table.Column<int>(type: "int", nullable: false),
                    AlgorithmDesignScore = table.Column<int>(type: "int", nullable: false),
                    ComplexityAnalysisScore = table.Column<int>(type: "int", nullable: false),
                    CodingScore = table.Column<int>(type: "int", nullable: false),
                    TestingScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeetcodeMockInterviewRound", x => x.LeetcodeMockInterviewRoundId);
                    table.ForeignKey(
                        name: "FK_LeetcodeMockInterviewRound_LeetcodeProblem_LeetcodeProblemId",
                        column: x => x.LeetcodeProblemId,
                        principalTable: "LeetcodeProblem",
                        principalColumn: "LeetcodeProblemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeetcodeMockInterviewRound_MockInterview_MockInterviewId",
                        column: x => x.MockInterviewId,
                        principalTable: "MockInterview",
                        principalColumn: "MockInterviewId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MockInterviewRound",
                columns: table => new
                {
                    MockInterviewRoundId = table.Column<string>(type: "varchar(16)", nullable: false),
                    MockInterviewId = table.Column<string>(type: "varchar(16)", nullable: false),
                    IsReviewedByInterviewee = table.Column<bool>(type: "boolean", nullable: false),
                    IntervieweeComment = table.Column<string>(type: "varchar(1000)", nullable: false),
                    BehaviouralMockInterviewRoundId = table.Column<string>(type: "varchar(32)", nullable: true),
                    LeetcodeMockInterviewRoundId = table.Column<string>(type: "varchar(32)", nullable: true),
                    CustomMockInterviewRoundId = table.Column<string>(type: "varchar(32)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockInterviewRound", x => x.MockInterviewRoundId);
                    table.ForeignKey(
                        name: "FK_MockInterviewRound_BehaviouralMockInterviewRound_Behavioura~",
                        column: x => x.BehaviouralMockInterviewRoundId,
                        principalTable: "BehaviouralMockInterviewRound",
                        principalColumn: "BehaviouralMockInterviewRoundId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MockInterviewRound_CustomMockInterviewRound_CustomMockInter~",
                        column: x => x.CustomMockInterviewRoundId,
                        principalTable: "CustomMockInterviewRound",
                        principalColumn: "CustomMockInterviewRoundId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MockInterviewRound_LeetcodeMockInterviewRound_LeetcodeMockI~",
                        column: x => x.LeetcodeMockInterviewRoundId,
                        principalTable: "LeetcodeMockInterviewRound",
                        principalColumn: "LeetcodeMockInterviewRoundId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MockInterviewRound_MockInterview_MockInterviewId",
                        column: x => x.MockInterviewId,
                        principalTable: "MockInterview",
                        principalColumn: "MockInterviewId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BehaviouralMockInterviewRound_MockInterviewId",
                table: "BehaviouralMockInterviewRound",
                column: "MockInterviewId");

            migrationBuilder.CreateIndex(
                name: "IX_BehaviouralMockInterviewRound_MockInterviewRoundId",
                table: "BehaviouralMockInterviewRound",
                column: "MockInterviewRoundId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomMockInterviewRound_MockInterviewId",
                table: "CustomMockInterviewRound",
                column: "MockInterviewId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomMockInterviewRound_MockInterviewRoundId",
                table: "CustomMockInterviewRound",
                column: "MockInterviewRoundId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomProblem_ProblemId",
                table: "CustomProblem",
                column: "ProblemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_SeasonId",
                table: "Enrollment",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_SeasonId_UserId",
                table: "Enrollment",
                columns: new[] { "SeasonId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_UserId",
                table: "Enrollment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LeetcodeMockInterviewRound_LeetcodeProblemId",
                table: "LeetcodeMockInterviewRound",
                column: "LeetcodeProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_LeetcodeMockInterviewRound_MockInterviewId",
                table: "LeetcodeMockInterviewRound",
                column: "MockInterviewId");

            migrationBuilder.CreateIndex(
                name: "IX_LeetcodeMockInterviewRound_MockInterviewRoundId",
                table: "LeetcodeMockInterviewRound",
                column: "MockInterviewRoundId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeetcodeProblem_ProblemId",
                table: "LeetcodeProblem",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_LeetcodeProblemCategoryMapping_LeetcodeProblemEntityLeetcod~",
                table: "LeetcodeProblemCategoryMapping",
                column: "LeetcodeProblemEntityLeetcodeProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_Mentorship_MenteeEnrollmentId",
                table: "Mentorship",
                column: "MenteeEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Mentorship_MentorEnrollmentId",
                table: "Mentorship",
                column: "MentorEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MockInterview_EnrollmentId",
                table: "MockInterview",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MockInterview_IntervieweeUserId",
                table: "MockInterview",
                column: "IntervieweeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MockInterview_InterviewerUserId",
                table: "MockInterview",
                column: "InterviewerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MockInterviewRound_BehaviouralMockInterviewRoundId",
                table: "MockInterviewRound",
                column: "BehaviouralMockInterviewRoundId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MockInterviewRound_CustomMockInterviewRoundId",
                table: "MockInterviewRound",
                column: "CustomMockInterviewRoundId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MockInterviewRound_LeetcodeMockInterviewRoundId",
                table: "MockInterviewRound",
                column: "LeetcodeMockInterviewRoundId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MockInterviewRound_MockInterviewId",
                table: "MockInterviewRound",
                column: "MockInterviewId");

            migrationBuilder.CreateIndex(
                name: "IX_ProblemAttempt_CustomProblemId",
                table: "ProblemAttempt",
                column: "CustomProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProblemAttempt_EnrollmentId",
                table: "ProblemAttempt",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProblemAttempt_LeetcodeProblemId",
                table: "ProblemAttempt",
                column: "LeetcodeProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProblemAttempt_UserId",
                table: "ProblemAttempt",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Season_Slug",
                table: "Season",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BehaviouralMockInterviewRound_MockInterviewRound_MockInterv~",
                table: "BehaviouralMockInterviewRound",
                column: "MockInterviewRoundId",
                principalTable: "MockInterviewRound",
                principalColumn: "MockInterviewRoundId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomMockInterviewRound_MockInterviewRound_MockInterviewRo~",
                table: "CustomMockInterviewRound",
                column: "MockInterviewRoundId",
                principalTable: "MockInterviewRound",
                principalColumn: "MockInterviewRoundId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LeetcodeMockInterviewRound_MockInterviewRound_MockInterview~",
                table: "LeetcodeMockInterviewRound",
                column: "MockInterviewRoundId",
                principalTable: "MockInterviewRound",
                principalColumn: "MockInterviewRoundId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BehaviouralMockInterviewRound_MockInterviewRound_MockInterv~",
                table: "BehaviouralMockInterviewRound");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomMockInterviewRound_MockInterviewRound_MockInterviewRo~",
                table: "CustomMockInterviewRound");

            migrationBuilder.DropForeignKey(
                name: "FK_LeetcodeMockInterviewRound_MockInterviewRound_MockInterview~",
                table: "LeetcodeMockInterviewRound");

            migrationBuilder.DropTable(
                name: "LeetcodeProblemCategoryMapping");

            migrationBuilder.DropTable(
                name: "Mentorship");

            migrationBuilder.DropTable(
                name: "ProblemAttempt");

            migrationBuilder.DropTable(
                name: "LeetcodeProblemCategory");

            migrationBuilder.DropTable(
                name: "CustomProblem");

            migrationBuilder.DropTable(
                name: "MockInterviewRound");

            migrationBuilder.DropTable(
                name: "BehaviouralMockInterviewRound");

            migrationBuilder.DropTable(
                name: "CustomMockInterviewRound");

            migrationBuilder.DropTable(
                name: "LeetcodeMockInterviewRound");

            migrationBuilder.DropTable(
                name: "LeetcodeProblem");

            migrationBuilder.DropTable(
                name: "MockInterview");

            migrationBuilder.DropTable(
                name: "Problem");

            migrationBuilder.DropTable(
                name: "Enrollment");

            migrationBuilder.DropTable(
                name: "Season");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
