using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepUp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChallengeCreator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Challenges",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_CreatedByUserId",
                table: "Challenges",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Challenges_Users_CreatedByUserId",
                table: "Challenges",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Challenges_Users_CreatedByUserId",
                table: "Challenges");

            migrationBuilder.DropIndex(
                name: "IX_Challenges_CreatedByUserId",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Challenges");
        }
    }
}
