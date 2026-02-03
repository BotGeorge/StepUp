using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepUp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseTypeToActivityLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExerciseType",
                table: "ActivityLogs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExerciseType",
                table: "ActivityLogs");
        }
    }
}
