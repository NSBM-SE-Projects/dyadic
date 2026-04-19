using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dyadic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProposalEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProposalEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RelatedSupervisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposalEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProposalEvents_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProposalEvents_Proposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "Proposals",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProposalEvents_SupervisorProfiles_RelatedSupervisorId",
                        column: x => x.RelatedSupervisorId,
                        principalTable: "SupervisorProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProposalEvents_ActorUserId",
                table: "ProposalEvents",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalEvents_ProposalId",
                table: "ProposalEvents",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalEvents_RelatedSupervisorId",
                table: "ProposalEvents",
                column: "RelatedSupervisorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProposalEvents");
        }
    }
}
