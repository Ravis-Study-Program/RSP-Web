using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSPWebAPI.Migrations
{
  /// <inheritdoc />
  public partial class AddEmailAndSlugUniqueIndex : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropIndex(name: "IX_User_Email", table: "User");

      migrationBuilder.DropIndex(name: "IX_User_Slug", table: "User");

      migrationBuilder.CreateIndex(
        name: "IX_User_Email",
        table: "User",
        column: "Email",
        unique: true
      );

      migrationBuilder.CreateIndex(
        name: "IX_User_Slug",
        table: "User",
        column: "Slug",
        unique: true
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropIndex(name: "IX_User_Email", table: "User");

      migrationBuilder.DropIndex(name: "IX_User_Slug", table: "User");

      migrationBuilder.CreateIndex(name: "IX_User_Email", table: "User", column: "Email");

      migrationBuilder.CreateIndex(name: "IX_User_Slug", table: "User", column: "Slug");
    }
  }
}
