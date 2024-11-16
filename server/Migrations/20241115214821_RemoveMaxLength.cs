using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSPWebAPI.Migrations
{
  /// <inheritdoc />
  public partial class RemoveMaxLength : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<string>(
        name: "Notes",
        table: "ProblemAttempt",
        type: "varchar(10000)",
        nullable: false,
        oldClrType: typeof(string),
        oldType: "varchar(16)",
        oldMaxLength: 10000
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<string>(
        name: "Notes",
        table: "ProblemAttempt",
        type: "varchar(16)",
        maxLength: 10000,
        nullable: false,
        oldClrType: typeof(string),
        oldType: "varchar(10000)"
      );
    }
  }
}
