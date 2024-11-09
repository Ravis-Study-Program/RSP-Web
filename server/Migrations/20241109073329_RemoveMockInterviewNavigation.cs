using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSPWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMockInterviewNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BehaviouralMockInterviewRound_MockInterviewRound_MockInterv~",
                table: "BehaviouralMockInterviewRound");

            migrationBuilder.DropForeignKey(
                name: "FK_BehaviouralMockInterviewRound_MockInterview_MockInterviewId",
                table: "BehaviouralMockInterviewRound");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomMockInterviewRound_MockInterviewRound_MockInterviewRo~",
                table: "CustomMockInterviewRound");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomMockInterviewRound_MockInterview_MockInterviewId",
                table: "CustomMockInterviewRound");

            migrationBuilder.DropForeignKey(
                name: "FK_LeetcodeMockInterviewRound_MockInterviewRound_MockInterview~",
                table: "LeetcodeMockInterviewRound");

            migrationBuilder.DropForeignKey(
                name: "FK_LeetcodeMockInterviewRound_MockInterview_MockInterviewId",
                table: "LeetcodeMockInterviewRound");

            migrationBuilder.DropIndex(
                name: "IX_LeetcodeMockInterviewRound_MockInterviewId",
                table: "LeetcodeMockInterviewRound");

            migrationBuilder.DropIndex(
                name: "IX_LeetcodeMockInterviewRound_MockInterviewRoundId",
                table: "LeetcodeMockInterviewRound");

            migrationBuilder.DropIndex(
                name: "IX_CustomMockInterviewRound_MockInterviewId",
                table: "CustomMockInterviewRound");

            migrationBuilder.DropIndex(
                name: "IX_CustomMockInterviewRound_MockInterviewRoundId",
                table: "CustomMockInterviewRound");

            migrationBuilder.DropIndex(
                name: "IX_BehaviouralMockInterviewRound_MockInterviewId",
                table: "BehaviouralMockInterviewRound");

            migrationBuilder.DropIndex(
                name: "IX_BehaviouralMockInterviewRound_MockInterviewRoundId",
                table: "BehaviouralMockInterviewRound");

            migrationBuilder.DropColumn(
                name: "MockInterviewId",
                table: "LeetcodeMockInterviewRound");

            migrationBuilder.DropColumn(
                name: "MockInterviewRoundId",
                table: "LeetcodeMockInterviewRound");

            migrationBuilder.DropColumn(
                name: "MockInterviewId",
                table: "CustomMockInterviewRound");

            migrationBuilder.DropColumn(
                name: "MockInterviewRoundId",
                table: "CustomMockInterviewRound");

            migrationBuilder.DropColumn(
                name: "MockInterviewId",
                table: "BehaviouralMockInterviewRound");

            migrationBuilder.DropColumn(
                name: "MockInterviewRoundId",
                table: "BehaviouralMockInterviewRound");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MockInterviewId",
                table: "LeetcodeMockInterviewRound",
                type: "varchar(16)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MockInterviewRoundId",
                table: "LeetcodeMockInterviewRound",
                type: "varchar(16)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MockInterviewId",
                table: "CustomMockInterviewRound",
                type: "varchar(16)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MockInterviewRoundId",
                table: "CustomMockInterviewRound",
                type: "varchar(16)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MockInterviewId",
                table: "BehaviouralMockInterviewRound",
                type: "varchar(16)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MockInterviewRoundId",
                table: "BehaviouralMockInterviewRound",
                type: "varchar(16)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_LeetcodeMockInterviewRound_MockInterviewId",
                table: "LeetcodeMockInterviewRound",
                column: "MockInterviewId");

            migrationBuilder.CreateIndex(
                name: "IX_LeetcodeMockInterviewRound_MockInterviewRoundId",
                table: "LeetcodeMockInterviewRound",
                column: "MockInterviewRoundId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomMockInterviewRound_MockInterviewId",
                table: "CustomMockInterviewRound",
                column: "MockInterviewId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomMockInterviewRound_MockInterviewRoundId",
                table: "CustomMockInterviewRound",
                column: "MockInterviewRoundId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BehaviouralMockInterviewRound_MockInterviewId",
                table: "BehaviouralMockInterviewRound",
                column: "MockInterviewId");

            migrationBuilder.CreateIndex(
                name: "IX_BehaviouralMockInterviewRound_MockInterviewRoundId",
                table: "BehaviouralMockInterviewRound",
                column: "MockInterviewRoundId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BehaviouralMockInterviewRound_MockInterviewRound_MockInterv~",
                table: "BehaviouralMockInterviewRound",
                column: "MockInterviewRoundId",
                principalTable: "MockInterviewRound",
                principalColumn: "MockInterviewRoundId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BehaviouralMockInterviewRound_MockInterview_MockInterviewId",
                table: "BehaviouralMockInterviewRound",
                column: "MockInterviewId",
                principalTable: "MockInterview",
                principalColumn: "MockInterviewId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomMockInterviewRound_MockInterviewRound_MockInterviewRo~",
                table: "CustomMockInterviewRound",
                column: "MockInterviewRoundId",
                principalTable: "MockInterviewRound",
                principalColumn: "MockInterviewRoundId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomMockInterviewRound_MockInterview_MockInterviewId",
                table: "CustomMockInterviewRound",
                column: "MockInterviewId",
                principalTable: "MockInterview",
                principalColumn: "MockInterviewId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LeetcodeMockInterviewRound_MockInterviewRound_MockInterview~",
                table: "LeetcodeMockInterviewRound",
                column: "MockInterviewRoundId",
                principalTable: "MockInterviewRound",
                principalColumn: "MockInterviewRoundId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LeetcodeMockInterviewRound_MockInterview_MockInterviewId",
                table: "LeetcodeMockInterviewRound",
                column: "MockInterviewId",
                principalTable: "MockInterview",
                principalColumn: "MockInterviewId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
