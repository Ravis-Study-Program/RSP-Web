using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RSPWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class Mentorships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("74a85bfe-792d-4531-a192-d08bc411a8ca"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("b5873e18-0ded-4b26-95a3-b1a11697b7d2"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("df22a97d-dc64-41f1-a70d-7ab2ea72129c"));

            migrationBuilder.CreateTable(
                name: "Mentorships",
                columns: table => new
                {
                    MentorshipId = table.Column<Guid>(type: "uuid", nullable: false),
                    MentorEnrollmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    MenteeEnrollmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mentorships", x => x.MentorshipId);
                    table.ForeignKey(
                        name: "FK_Mentorships_Enrollments_MenteeEnrollmentId",
                        column: x => x.MenteeEnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "EnrollmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Mentorships_Enrollments_MentorEnrollmentId",
                        column: x => x.MentorEnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "EnrollmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Name" },
                values: new object[,]
                {
                    { new Guid("c9ce1f8d-e826-40e4-a36d-bd4126570cdb"), "Student" },
                    { new Guid("d0a0fc50-4a1c-4ef2-b1d0-aefb556488b2"), "Mentor" },
                    { new Guid("f6a00527-0e4e-4b60-a7bb-0372cab56bd1"), "Coordinator" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mentorships_MenteeEnrollmentId",
                table: "Mentorships",
                column: "MenteeEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Mentorships_MentorEnrollmentId",
                table: "Mentorships",
                column: "MentorEnrollmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Mentorships");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("c9ce1f8d-e826-40e4-a36d-bd4126570cdb"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("d0a0fc50-4a1c-4ef2-b1d0-aefb556488b2"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("f6a00527-0e4e-4b60-a7bb-0372cab56bd1"));

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Name" },
                values: new object[,]
                {
                    { new Guid("74a85bfe-792d-4531-a192-d08bc411a8ca"), "Mentor" },
                    { new Guid("b5873e18-0ded-4b26-95a3-b1a11697b7d2"), "Student" },
                    { new Guid("df22a97d-dc64-41f1-a70d-7ab2ea72129c"), "Coordinator" }
                });
        }
    }
}
