using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSPWebAPI.Migrations
{
  /// <inheritdoc />
  public partial class MakeAllIdToBeVarchar16 : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<string>(
        name: "Slug",
        table: "Season",
        type: "varchar(16)",
        nullable: false,
        oldClrType: typeof(string),
        oldType: "varchar(32)"
      );

      migrationBuilder.AlterColumn<string>(
        name: "LeetcodeMockInterviewRoundId",
        table: "MockInterviewRound",
        type: "varchar(16)",
        nullable: true,
        oldClrType: typeof(string),
        oldType: "varchar(32)",
        oldNullable: true
      );

      migrationBuilder.AlterColumn<string>(
        name: "CustomMockInterviewRoundId",
        table: "MockInterviewRound",
        type: "varchar(16)",
        nullable: true,
        oldClrType: typeof(string),
        oldType: "varchar(32)",
        oldNullable: true
      );

      migrationBuilder.AlterColumn<string>(
        name: "BehaviouralMockInterviewRoundId",
        table: "MockInterviewRound",
        type: "varchar(16)",
        nullable: true,
        oldClrType: typeof(string),
        oldType: "varchar(32)",
        oldNullable: true
      );

      migrationBuilder.AlterColumn<string>(
        name: "LeetcodeProblemCategoriesLeetcodeProblemCategoryId",
        table: "LeetcodeProblemCategoryMapping",
        type: "varchar(16)",
        nullable: false,
        oldClrType: typeof(string),
        oldType: "varchar(32)"
      );

      migrationBuilder.AlterColumn<string>(
        name: "LeetcodeProblemCategoryId",
        table: "LeetcodeProblemCategory",
        type: "varchar(16)",
        nullable: false,
        oldClrType: typeof(string),
        oldType: "varchar(32)"
      );

      migrationBuilder.AlterColumn<string>(
        name: "LeetcodeMockInterviewRoundId",
        table: "LeetcodeMockInterviewRound",
        type: "varchar(16)",
        nullable: false,
        oldClrType: typeof(string),
        oldType: "varchar(32)"
      );

      migrationBuilder.AlterColumn<string>(
        name: "CustomMockInterviewRoundId",
        table: "CustomMockInterviewRound",
        type: "varchar(16)",
        nullable: false,
        oldClrType: typeof(string),
        oldType: "varchar(32)"
      );

      migrationBuilder.AlterColumn<string>(
        name: "BehaviouralMockInterviewRoundId",
        table: "BehaviouralMockInterviewRound",
        type: "varchar(16)",
        nullable: false,
        oldClrType: typeof(string),
        oldType: "varchar(32)"
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<string>(
        name: "Slug",
        table: "Season",
        type: "varchar(32)",
        nullable: false,
        oldClrType: typeof(string),
        oldType: "varchar(16)"
      );

      migrationBuilder.AlterColumn<string>(
        name: "LeetcodeMockInterviewRoundId",
        table: "MockInterviewRound",
        type: "varchar(32)",
        nullable: true,
        oldClrType: typeof(string),
        oldType: "varchar(16)",
        oldNullable: true
      );

      migrationBuilder.AlterColumn<string>(
        name: "CustomMockInterviewRoundId",
        table: "MockInterviewRound",
        type: "varchar(32)",
        nullable: true,
        oldClrType: typeof(string),
        oldType: "varchar(16)",
        oldNullable: true
      );

      migrationBuilder.AlterColumn<string>(
        name: "BehaviouralMockInterviewRoundId",
        table: "MockInterviewRound",
        type: "varchar(32)",
        nullable: true,
        oldClrType: typeof(string),
        oldType: "varchar(16)",
        oldNullable: true
      );

      migrationBuilder.AlterColumn<string>(
        name: "LeetcodeProblemCategoriesLeetcodeProblemCategoryId",
        table: "LeetcodeProblemCategoryMapping",
        type: "varchar(32)",
        nullable: false,
        oldClrType: typeof(string),
        oldType: "varchar(16)"
      );

      migrationBuilder.AlterColumn<string>(
        name: "LeetcodeProblemCategoryId",
        table: "LeetcodeProblemCategory",
        type: "varchar(32)",
        nullable: false,
        oldClrType: typeof(string),
        oldType: "varchar(16)"
      );

      migrationBuilder.AlterColumn<string>(
        name: "LeetcodeMockInterviewRoundId",
        table: "LeetcodeMockInterviewRound",
        type: "varchar(32)",
        nullable: false,
        oldClrType: typeof(string),
        oldType: "varchar(16)"
      );

      migrationBuilder.AlterColumn<string>(
        name: "CustomMockInterviewRoundId",
        table: "CustomMockInterviewRound",
        type: "varchar(32)",
        nullable: false,
        oldClrType: typeof(string),
        oldType: "varchar(16)"
      );

      migrationBuilder.AlterColumn<string>(
        name: "BehaviouralMockInterviewRoundId",
        table: "BehaviouralMockInterviewRound",
        type: "varchar(32)",
        nullable: false,
        oldClrType: typeof(string),
        oldType: "varchar(16)"
      );
    }
  }
}
