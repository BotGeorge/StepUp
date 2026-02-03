using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepUp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddParentActivityLogLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentActivityLogId",
                table: "ActivityLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_ParentActivityLogId",
                table: "ActivityLogs",
                column: "ParentActivityLogId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityLogs_ActivityLogs_ParentActivityLogId",
                table: "ActivityLogs",
                column: "ParentActivityLogId",
                principalTable: "ActivityLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityLogs_ActivityLogs_ParentActivityLogId",
                table: "ActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_ActivityLogs_ParentActivityLogId",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "ParentActivityLogId",
                table: "ActivityLogs");
        }
    }
}
