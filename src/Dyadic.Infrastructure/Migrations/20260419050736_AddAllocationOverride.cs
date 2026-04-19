using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dyadic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAllocationOverride : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AllocationOverrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerformedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldSupervisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NewSupervisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllocationOverrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllocationOverrides_AspNetUsers_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AllocationOverrides_Proposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "Proposals",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllocationOverrides_PerformedByUserId",
                table: "AllocationOverrides",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationOverrides_ProposalId",
                table: "AllocationOverrides",
                column: "ProposalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllocationOverrides");
        }
    }
}
