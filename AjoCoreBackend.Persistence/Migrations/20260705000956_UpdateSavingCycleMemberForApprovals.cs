using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjoCoreBackend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSavingCycleMemberForApprovals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "NombaVirtualAccountId",
                table: "SavingCycleMembers",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_SavingCycleMembers_UserId",
                table: "SavingCycleMembers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SavingCycleMembers_Traders_UserId",
                table: "SavingCycleMembers",
                column: "UserId",
                principalTable: "Traders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SavingCycleMembers_Traders_UserId",
                table: "SavingCycleMembers");

            migrationBuilder.DropIndex(
                name: "IX_SavingCycleMembers_UserId",
                table: "SavingCycleMembers");

            migrationBuilder.AlterColumn<Guid>(
                name: "NombaVirtualAccountId",
                table: "SavingCycleMembers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
