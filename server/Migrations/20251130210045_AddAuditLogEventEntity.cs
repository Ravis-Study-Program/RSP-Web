using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSPWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogEventEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditEvent",
                columns: table => new
                {
                    AuditId = table.Column<string>(type: "varchar(16)", nullable: false),
                    ModifiedByUserId = table.Column<string>(type: "varchar(16)", nullable: false),
                    AuditedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    TableName = table.Column<string>(type: "varchar(100)", nullable: false),
                    AffectedEntityKey = table.Column<string>(type: "varchar(16)", nullable: false),
                    ChangeState = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEvent", x => x.AuditId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvent_TableName_AffectedEntityKey",
                table: "AuditEvent",
                columns: new[] { "TableName", "AffectedEntityKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditEvent");
        }
    }
}
