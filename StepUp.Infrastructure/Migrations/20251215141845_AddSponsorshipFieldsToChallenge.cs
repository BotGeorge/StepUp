using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepUp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSponsorshipFieldsToChallenge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSponsored",
                table: "Challenges",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Prize",
                table: "Challenges",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SponsorId",
                table: "Challenges",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_SponsorId",
                table: "Challenges",
                column: "SponsorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Challenges_Users_SponsorId",
                table: "Challenges",
                column: "SponsorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Challenges_Users_SponsorId",
                table: "Challenges");

            migrationBuilder.DropIndex(
                name: "IX_Challenges_SponsorId",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "IsSponsored",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "Prize",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "SponsorId",
                table: "Challenges");
        }
    }
}
