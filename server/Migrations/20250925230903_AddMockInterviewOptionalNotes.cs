using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSPWebAPI.Migrations
{
  /// <inheritdoc />
  public partial class AddMockInterviewOptionalNotes : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<string>(
        name: "Notes",
        table: "ProblemAttempt",
        type: "varchar(10000)",
        nullable: true,
        oldClrType: typeof(string),
        oldType: "varchar(10000)"
      );

      migrationBuilder.AddColumn<string>(
        name: "Notes",
        table: "MockInterview",
        type: "varchar(10000)",
        nullable: true
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(name: "Notes", table: "MockInterview");

      migrationBuilder.AlterColumn<string>(
        name: "Notes",
        table: "ProblemAttempt",
        type: "varchar(10000)",
        nullable: false,
        defaultValue: "",
        oldClrType: typeof(string),
        oldType: "varchar(10000)",
        oldNullable: true
      );
    }
  }
}
