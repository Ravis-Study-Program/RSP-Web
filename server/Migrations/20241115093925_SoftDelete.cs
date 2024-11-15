using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSPWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class SoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "User",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Season",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "ProblemAttempt",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Link",
                table: "Problem",
                type: "varchar(510)",
                maxLength: 510,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 510);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Problem",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "MockInterviewRound",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "MockInterview",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Mentorship",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "LeetcodeProblemCategory",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "LeetcodeProblem",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "LeetcodeMockInterviewRound",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Enrollment",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "CustomProblem",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "CustomMockInterviewRound",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "BehaviouralMockInterviewRound",
                type: "timestamptz",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "User");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "Season");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "ProblemAttempt");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "Problem");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "MockInterviewRound");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "MockInterview");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "Mentorship");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "LeetcodeProblemCategory");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "LeetcodeProblem");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "LeetcodeMockInterviewRound");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "CustomProblem");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "CustomMockInterviewRound");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "BehaviouralMockInterviewRound");

            migrationBuilder.AlterColumn<string>(
                name: "Link",
                table: "Problem",
                type: "varchar(255)",
                maxLength: 510,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(510)",
                oldMaxLength: 510);
        }
    }
}
