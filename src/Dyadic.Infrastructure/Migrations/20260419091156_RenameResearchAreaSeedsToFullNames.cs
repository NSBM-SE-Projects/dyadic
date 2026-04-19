using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dyadic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameResearchAreaSeedsToFullNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(table: "ResearchAreas",
                keyColumn: "Name", keyValue: "AI",
                column: "Name", value: "Artificial Intelligence"
            );

            migrationBuilder.UpdateData(table: "ResearchAreas",
                keyColumn: "Name", keyValue: "HCI",
                column: "Name", value: "Human-Computer Interaction"
            );

            migrationBuilder.UpdateData(table: "ResearchAreas",
                keyColumn: "Name", keyValue: "NLP",
                column: "Name", value: "Natural Language Processing"
            );

            migrationBuilder.UpdateData(table: "ResearchAreas",
                keyColumn: "Name", keyValue: "IoT",
                column: "Name", value: "Internet of Things"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(table: "ResearchAreas",
                keyColumn: "Name", keyValue: "Artificial Intelligence",
                column: "Name", value: "AI"
            );

            migrationBuilder.UpdateData(table: "ResearchAreas",
                keyColumn: "Name", keyValue: "Human-Computer Interaction",
                column: "Name", value: "HCI"
            );

            migrationBuilder.UpdateData(table: "ResearchAreas",
                keyColumn: "Name", keyValue: "Natural Language Processing",
                column: "Name", value: "NLP"
            );

            migrationBuilder.UpdateData(table: "ResearchAreas",
                keyColumn: "Name", keyValue: "Internet of Things",
                column: "Name", value: "IoT"
            );
        }
    }
}
