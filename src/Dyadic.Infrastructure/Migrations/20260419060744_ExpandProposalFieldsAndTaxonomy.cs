using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dyadic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandProposalFieldsAndTaxonomy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Proposals",
                newName: "Abstract");

            migrationBuilder.AddColumn<string>(
                name: "TechStack",
                table: "Proposals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ResearchAreaId",
                table: "Proposals",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ResearchAreas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchAreas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_ResearchAreaId",
                table: "Proposals",
                column: "ResearchAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchAreas_Name",
                table: "ResearchAreas",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Proposals_ResearchAreas_ResearchAreaId",
                table: "Proposals",
                column: "ResearchAreaId",
                principalTable: "ResearchAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var areas = new[]
            {
                "AI", "Machine Learning", "Cybersecurity", "Cloud Computing",
                "Software Engineering", "Data Science", "HCI", "Computer Vision",
                "NLP", "IoT", "Blockchain", "Mobile Development",
                "Web Development", "Game Development"
            };
            foreach (var name in areas)
            {
                migrationBuilder.InsertData(
                    table: "ResearchAreas",
                    columns: new[] { "Id", "Name", "IsActive", "CreatedAt" },
                    values: new object[] { Guid.NewGuid(), name, true, now });
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Proposals_ResearchAreas_ResearchAreaId",
                table: "Proposals");

            migrationBuilder.DropTable(
                name: "ResearchAreas");

            migrationBuilder.DropIndex(
                name: "IX_Proposals_ResearchAreaId",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "TechStack",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "ResearchAreaId",
                table: "Proposals");

            migrationBuilder.RenameColumn(
                name: "Abstract",
                table: "Proposals",
                newName: "Description");
        }
    }
}
