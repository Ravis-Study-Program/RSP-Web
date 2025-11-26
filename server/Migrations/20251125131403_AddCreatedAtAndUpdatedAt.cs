using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSPWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtAndUpdatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "User",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "User",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "SeasonWeek",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "SeasonWeek",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Season",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Season",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "ProblemAttempt",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "ProblemAttempt",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Problem",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Problem",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "MockInterviewRound",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "MockInterviewRound",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "MockInterview",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "MockInterview",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Mentorship",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Mentorship",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "LeetcodeProblemRecommendation",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "LeetcodeProblemRecommendation",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "LeetcodeProblemCategory",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "LeetcodeProblemCategory",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "LeetcodeProblem",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "LeetcodeProblem",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "LeetcodeMockInterviewRound",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "LeetcodeMockInterviewRound",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Enrollment",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Enrollment",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "CustomProblem",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "CustomProblem",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "CustomMockInterviewRound",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "CustomMockInterviewRound",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "BehaviouralMockInterviewRound",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "BehaviouralMockInterviewRound",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "User");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "User");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "SeasonWeek");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "SeasonWeek");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Season");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Season");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "ProblemAttempt");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "ProblemAttempt");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Problem");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Problem");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "MockInterviewRound");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "MockInterviewRound");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "MockInterview");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "MockInterview");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Mentorship");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Mentorship");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "LeetcodeProblemRecommendation");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "LeetcodeProblemRecommendation");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "LeetcodeProblemCategory");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "LeetcodeProblemCategory");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "LeetcodeProblem");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "LeetcodeProblem");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "LeetcodeMockInterviewRound");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "LeetcodeMockInterviewRound");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "CustomProblem");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "CustomProblem");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "CustomMockInterviewRound");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "CustomMockInterviewRound");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "BehaviouralMockInterviewRound");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "BehaviouralMockInterviewRound");
        }
    }
}
