using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepUp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMetricTypeToActivityLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MetricType",
                table: "ActivityLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MetricType",
                table: "ActivityLogs");
        }
    }
}
