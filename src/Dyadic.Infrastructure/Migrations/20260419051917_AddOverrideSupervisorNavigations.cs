using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dyadic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOverrideSupervisorNavigations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AllocationOverrides_NewSupervisorId",
                table: "AllocationOverrides",
                column: "NewSupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationOverrides_OldSupervisorId",
                table: "AllocationOverrides",
                column: "OldSupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_AllocationOverrides_SupervisorProfiles_NewSupervisorId",
                table: "AllocationOverrides",
                column: "NewSupervisorId",
                principalTable: "SupervisorProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AllocationOverrides_SupervisorProfiles_OldSupervisorId",
                table: "AllocationOverrides",
                column: "OldSupervisorId",
                principalTable: "SupervisorProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AllocationOverrides_SupervisorProfiles_NewSupervisorId",
                table: "AllocationOverrides");

            migrationBuilder.DropForeignKey(
                name: "FK_AllocationOverrides_SupervisorProfiles_OldSupervisorId",
                table: "AllocationOverrides");

            migrationBuilder.DropIndex(
                name: "IX_AllocationOverrides_NewSupervisorId",
                table: "AllocationOverrides");

            migrationBuilder.DropIndex(
                name: "IX_AllocationOverrides_OldSupervisorId",
                table: "AllocationOverrides");
        }
    }
}
