using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RSPWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class LeetcodeAndCustomProblems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("4c092306-a9aa-4586-9531-8ac84ff6107a"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("5d99f8bc-a10d-472a-877b-e6713fefc518"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("cf89eb8d-c8c0-47eb-9452-f86dda8f2b0c"));

            migrationBuilder.CreateTable(
                name: "LeetcodeProblemDifficulties",
                columns: table => new
                {
                    LeetcodeProblemDifficultyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeetcodeProblemDifficulties", x => x.LeetcodeProblemDifficultyId);
                });

            migrationBuilder.CreateTable(
                name: "Problems",
                columns: table => new
                {
                    ProblemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Link = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Problems", x => x.ProblemId);
                });

            migrationBuilder.CreateTable(
                name: "CustomProblems",
                columns: table => new
                {
                    CustomProblemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Difficulty = table.Column<string>(type: "text", nullable: false),
                    Question = table.Column<string>(type: "text", nullable: false),
                    ProblemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomProblems", x => x.CustomProblemId);
                    table.ForeignKey(
                        name: "FK_CustomProblems_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "ProblemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeetcodeProblemCategories",
                columns: table => new
                {
                    LeetcodeProblemCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ProblemId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeetcodeProblemCategories", x => x.LeetcodeProblemCategoryId);
                    table.ForeignKey(
                        name: "FK_LeetcodeProblemCategories_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "ProblemId");
                });

            migrationBuilder.CreateTable(
                name: "LeetcodeProblems",
                columns: table => new
                {
                    LeetcodeProblemId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeetcodeProblemDifficultyId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPremium = table.Column<bool>(type: "boolean", nullable: false),
                    ProblemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeetcodeProblems", x => x.LeetcodeProblemId);
                    table.ForeignKey(
                        name: "FK_LeetcodeProblems_LeetcodeProblemDifficulties_LeetcodeProble~",
                        column: x => x.LeetcodeProblemDifficultyId,
                        principalTable: "LeetcodeProblemDifficulties",
                        principalColumn: "LeetcodeProblemDifficultyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeetcodeProblems_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "ProblemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeetcodeProblemLeetcodeProblemCategory",
                columns: table => new
                {
                    LeetcodeProblemCategoriesLeetcodeProblemCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeetcodeProblemsLeetcodeProblemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeetcodeProblemLeetcodeProblemCategory", x => new { x.LeetcodeProblemCategoriesLeetcodeProblemCategoryId, x.LeetcodeProblemsLeetcodeProblemId });
                    table.ForeignKey(
                        name: "FK_LeetcodeProblemLeetcodeProblemCategory_LeetcodeProblemCateg~",
                        column: x => x.LeetcodeProblemCategoriesLeetcodeProblemCategoryId,
                        principalTable: "LeetcodeProblemCategories",
                        principalColumn: "LeetcodeProblemCategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeetcodeProblemLeetcodeProblemCategory_LeetcodeProblems_Lee~",
                        column: x => x.LeetcodeProblemsLeetcodeProblemId,
                        principalTable: "LeetcodeProblems",
                        principalColumn: "LeetcodeProblemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomProblems_ProblemId",
                table: "CustomProblems",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_LeetcodeProblemCategories_ProblemId",
                table: "LeetcodeProblemCategories",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_LeetcodeProblemLeetcodeProblemCategory_LeetcodeProblemsLeet~",
                table: "LeetcodeProblemLeetcodeProblemCategory",
                column: "LeetcodeProblemsLeetcodeProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_LeetcodeProblems_LeetcodeProblemDifficultyId",
                table: "LeetcodeProblems",
                column: "LeetcodeProblemDifficultyId");

            migrationBuilder.CreateIndex(
                name: "IX_LeetcodeProblems_ProblemId",
                table: "LeetcodeProblems",
                column: "ProblemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomProblems");

            migrationBuilder.DropTable(
                name: "LeetcodeProblemLeetcodeProblemCategory");

            migrationBuilder.DropTable(
                name: "LeetcodeProblemCategories");

            migrationBuilder.DropTable(
                name: "LeetcodeProblems");

            migrationBuilder.DropTable(
                name: "LeetcodeProblemDifficulties");

            migrationBuilder.DropTable(
                name: "Problems");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Name" },
                values: new object[,]
                {
                    { new Guid("4c092306-a9aa-4586-9531-8ac84ff6107a"), "Coordinator" },
                    { new Guid("5d99f8bc-a10d-472a-877b-e6713fefc518"), "Mentor" },
                    { new Guid("cf89eb8d-c8c0-47eb-9452-f86dda8f2b0c"), "Student" }
                });
        }
    }
}
