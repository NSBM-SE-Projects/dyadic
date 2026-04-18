using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dyadic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSupervisorResearchAreas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResearchAreas",
                table: "SupervisorProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResearchAreas",
                table: "SupervisorProfiles");
        }
    }
}
