using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSPWebAPI.Migrations
{
  /// <inheritdoc />
  public partial class RemoveUniqueIndex : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropIndex(name: "IX_User_Email", table: "User");

      migrationBuilder.DropIndex(name: "IX_Season_Slug", table: "Season");

      migrationBuilder.DropIndex(name: "IX_Enrollment_SeasonId_UserId", table: "Enrollment");

      migrationBuilder.CreateIndex(name: "IX_User_Email", table: "User", column: "Email");

      migrationBuilder.CreateIndex(name: "IX_Season_Slug", table: "Season", column: "Slug");

      migrationBuilder.CreateIndex(
        name: "IX_Enrollment_SeasonId_UserId",
        table: "Enrollment",
        columns: new[] { "SeasonId", "UserId" }
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropIndex(name: "IX_User_Email", table: "User");

      migrationBuilder.DropIndex(name: "IX_Season_Slug", table: "Season");

      migrationBuilder.DropIndex(name: "IX_Enrollment_SeasonId_UserId", table: "Enrollment");

      migrationBuilder.CreateIndex(
        name: "IX_User_Email",
        table: "User",
        column: "Email",
        unique: true
      );

      migrationBuilder.CreateIndex(
        name: "IX_Season_Slug",
        table: "Season",
        column: "Slug",
        unique: true
      );

      migrationBuilder.CreateIndex(
        name: "IX_Enrollment_SeasonId_UserId",
        table: "Enrollment",
        columns: new[] { "SeasonId", "UserId" },
        unique: true
      );
    }
  }
}
