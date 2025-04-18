using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSPWebAPI.Migrations
{
  /// <inheritdoc />
  public partial class AddUserSlug : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<string>(
        name: "Slug",
        table: "User",
        type: "varchar(100)",
        nullable: false,
        defaultValue: ""
      );

      migrationBuilder.Sql(
        @"
                UPDATE ""User""
                SET ""Slug"" = LOWER(REPLACE(""Name"", ' ', '-'))
                WHERE ""Name"" IS NOT NULL;
            "
      );

      migrationBuilder.CreateIndex(name: "IX_User_Slug", table: "User", column: "Slug");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropIndex(name: "IX_User_Slug", table: "User");

      migrationBuilder.DropColumn(name: "Slug", table: "User");
    }
  }
}
