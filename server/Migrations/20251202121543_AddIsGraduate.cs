using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSPWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddIsGraduate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsGraduate",
                table: "User",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Update existing users to set IsGraduate = true for those enrolled in completed seasons
            migrationBuilder.Sql(@"
                UPDATE ""User"" 
                SET ""IsGraduate"" = true 
                WHERE ""UserId"" IN (
                    SELECT DISTINCT e.""UserId"" 
                    FROM ""Enrollment"" e 
                    INNER JOIN ""Season"" s ON e.""SeasonId"" = s.""SeasonId"" 
                    WHERE s.""EndDateInclusiveUTC"" < NOW() 
                    AND e.""DeletedAtUtc"" IS NULL 
                    AND s.""DeletedAtUtc"" IS NULL
                    AND ""User"".""DeletedAtUtc"" IS NULL
                )");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGraduate",
                table: "User");
        }
    }
}
