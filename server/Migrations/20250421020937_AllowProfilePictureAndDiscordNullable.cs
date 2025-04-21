using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSPWebAPI.Migrations
{
  /// <inheritdoc />
  public partial class AllowProfilePictureAndDiscordNullable : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<string>(
        name: "ProfileImage",
        table: "User",
        type: "varchar(255)",
        nullable: true,
        oldClrType: typeof(string),
        oldType: "varchar(255)"
      );

      migrationBuilder.AlterColumn<string>(
        name: "DiscordId",
        table: "User",
        type: "varchar(16)",
        nullable: true,
        oldClrType: typeof(string),
        oldType: "varchar(16)"
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<string>(
        name: "ProfileImage",
        table: "User",
        type: "varchar(255)",
        nullable: false,
        defaultValue: "",
        oldClrType: typeof(string),
        oldType: "varchar(255)",
        oldNullable: true
      );

      migrationBuilder.AlterColumn<string>(
        name: "DiscordId",
        table: "User",
        type: "varchar(16)",
        nullable: false,
        defaultValue: "",
        oldClrType: typeof(string),
        oldType: "varchar(16)",
        oldNullable: true
      );
    }
  }
}
