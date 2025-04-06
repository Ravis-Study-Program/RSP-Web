using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSPWebAPI.Migrations
{
  /// <inheritdoc />
  public partial class AddLeetcodeNumber : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<int>(
        name: "LeetcodeNumber",
        table: "LeetcodeProblem",
        type: "int",
        nullable: false,
        defaultValue: 0
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(name: "LeetcodeNumber", table: "LeetcodeProblem");
    }
  }
}
