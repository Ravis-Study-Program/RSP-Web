using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSPWebAPI.Migrations
{
  /// <inheritdoc />
  public partial class MockInterviewSeason : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
        name: "FK_MockInterview_Enrollment_EnrollmentId",
        table: "MockInterview"
      );

      migrationBuilder.RenameColumn(
        name: "EnrollmentId",
        table: "MockInterview",
        newName: "SeasonId"
      );

      migrationBuilder.RenameIndex(
        name: "IX_MockInterview_EnrollmentId",
        table: "MockInterview",
        newName: "IX_MockInterview_SeasonId"
      );

      migrationBuilder.AddForeignKey(
        name: "FK_MockInterview_Season_SeasonId",
        table: "MockInterview",
        column: "SeasonId",
        principalTable: "Season",
        principalColumn: "SeasonId",
        onDelete: ReferentialAction.SetNull
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
        name: "FK_MockInterview_Season_SeasonId",
        table: "MockInterview"
      );

      migrationBuilder.RenameColumn(
        name: "SeasonId",
        table: "MockInterview",
        newName: "EnrollmentId"
      );

      migrationBuilder.RenameIndex(
        name: "IX_MockInterview_SeasonId",
        table: "MockInterview",
        newName: "IX_MockInterview_EnrollmentId"
      );

      migrationBuilder.AddForeignKey(
        name: "FK_MockInterview_Enrollment_EnrollmentId",
        table: "MockInterview",
        column: "EnrollmentId",
        principalTable: "Enrollment",
        principalColumn: "EnrollmentId",
        onDelete: ReferentialAction.SetNull
      );
    }
  }
}
