using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RSPWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeasonSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("c2857a7f-c11b-43a2-a519-053be8531eee"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("f4cc0ff5-3573-45dd-a92e-1ec75aa21a1e"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("f9cc90f2-3bcd-4ae5-992f-707871be1fc3"));

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Seasons",
                type: "text",
                nullable: false,
                defaultValue: "");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Seasons");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Name" },
                values: new object[,]
                {
                    { new Guid("c2857a7f-c11b-43a2-a519-053be8531eee"), "Mentor" },
                    { new Guid("f4cc0ff5-3573-45dd-a92e-1ec75aa21a1e"), "Coordinator" },
                    { new Guid("f9cc90f2-3bcd-4ae5-992f-707871be1fc3"), "Student" }
                });
        }
    }
}
