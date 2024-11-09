using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSPWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class TimestampWithTimeZone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDateInclusiveUTC",
                table: "Season",
                type: "timestamptz",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDateInclusiveUTC",
                table: "Season",
                type: "timestamptz",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp");

            migrationBuilder.AlterColumn<string>(
                name: "LeetcodeProblemId",
                table: "ProblemAttempt",
                type: "varchar(16)",
                maxLength: 32,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(16)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EnrollmentId",
                table: "ProblemAttempt",
                type: "varchar(16)",
                maxLength: 32,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(16)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomProblemId",
                table: "ProblemAttempt",
                type: "varchar(16)",
                maxLength: 32,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(16)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "AttemptStartDateUtc",
                table: "ProblemAttempt",
                type: "timestamptz",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp");

            migrationBuilder.AlterColumn<string>(
                name: "Link",
                table: "Problem",
                type: "varchar(255)",
                maxLength: 510,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 510,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LeetcodeMockInterviewRoundId",
                table: "MockInterviewRound",
                type: "varchar(32)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(32)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomMockInterviewRoundId",
                table: "MockInterviewRound",
                type: "varchar(32)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(32)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BehaviouralMockInterviewRoundId",
                table: "MockInterviewRound",
                type: "varchar(32)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(32)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "MockInterview",
                type: "timestamptz",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDateInclusiveUTC",
                table: "Season",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamptz");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDateInclusiveUTC",
                table: "Season",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamptz");

            migrationBuilder.AlterColumn<string>(
                name: "LeetcodeProblemId",
                table: "ProblemAttempt",
                type: "varchar(16)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(16)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "EnrollmentId",
                table: "ProblemAttempt",
                type: "varchar(16)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(16)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "CustomProblemId",
                table: "ProblemAttempt",
                type: "varchar(16)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(16)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<DateTime>(
                name: "AttemptStartDateUtc",
                table: "ProblemAttempt",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamptz");

            migrationBuilder.AlterColumn<string>(
                name: "Link",
                table: "Problem",
                type: "varchar(255)",
                maxLength: 510,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 510);

            migrationBuilder.AlterColumn<string>(
                name: "LeetcodeMockInterviewRoundId",
                table: "MockInterviewRound",
                type: "varchar(32)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(32)");

            migrationBuilder.AlterColumn<string>(
                name: "CustomMockInterviewRoundId",
                table: "MockInterviewRound",
                type: "varchar(32)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(32)");

            migrationBuilder.AlterColumn<string>(
                name: "BehaviouralMockInterviewRoundId",
                table: "MockInterviewRound",
                type: "varchar(32)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(32)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "MockInterview",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamptz");
        }
    }
}
