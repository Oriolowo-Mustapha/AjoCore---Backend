using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjoCoreBackend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SavingCycles_SavingCycleMembers_IndividualOwnerId1",
                table: "SavingCycles");

            migrationBuilder.DropIndex(
                name: "IX_SavingCycles_IndividualOwnerId1",
                table: "SavingCycles");

            migrationBuilder.DropColumn(
                name: "IndividualOwnerId",
                table: "SavingCycles");

            migrationBuilder.DropColumn(
                name: "IndividualOwnerId1",
                table: "SavingCycles");

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "CooperativeAdmins",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                table: "CooperativeAdmins",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "CooperativeAdmins");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                table: "CooperativeAdmins");

            migrationBuilder.AddColumn<Guid>(
                name: "IndividualOwnerId",
                table: "SavingCycles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IndividualOwnerId1",
                table: "SavingCycles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavingCycles_IndividualOwnerId1",
                table: "SavingCycles",
                column: "IndividualOwnerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_SavingCycles_SavingCycleMembers_IndividualOwnerId1",
                table: "SavingCycles",
                column: "IndividualOwnerId1",
                principalTable: "SavingCycleMembers",
                principalColumn: "Id");
        }
    }
}
