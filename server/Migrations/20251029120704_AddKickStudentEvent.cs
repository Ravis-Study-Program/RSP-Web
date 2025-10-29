using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSPWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddKickStudentEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KickStudentEvent",
                columns: table => new
                {
                    KickStudentEventId = table.Column<string>(type: "varchar(16)", nullable: false),
                    KickedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    MentorId = table.Column<string>(type: "varchar(16)", nullable: false),
                    StudentId = table.Column<string>(type: "varchar(16)", nullable: false),
                    SeasonId = table.Column<string>(type: "varchar(16)", nullable: false),
                    KickReason = table.Column<string>(type: "text", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KickStudentEvent", x => x.KickStudentEventId);
                    table.ForeignKey(
                        name: "FK_KickStudentEvent_Season_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Season",
                        principalColumn: "SeasonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KickStudentEvent_User_MentorId",
                        column: x => x.MentorId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KickStudentEvent_User_StudentId",
                        column: x => x.StudentId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KickStudentEvent_KickedAtUtc",
                table: "KickStudentEvent",
                column: "KickedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_KickStudentEvent_MentorId",
                table: "KickStudentEvent",
                column: "MentorId");

            migrationBuilder.CreateIndex(
                name: "IX_KickStudentEvent_SeasonId",
                table: "KickStudentEvent",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_KickStudentEvent_SeasonId_KickedAtUtc",
                table: "KickStudentEvent",
                columns: new[] { "SeasonId", "KickedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_KickStudentEvent_StudentId",
                table: "KickStudentEvent",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_KickStudentEvent_StudentId_KickedAtUtc",
                table: "KickStudentEvent",
                columns: new[] { "StudentId", "KickedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KickStudentEvent");
        }
    }
}
