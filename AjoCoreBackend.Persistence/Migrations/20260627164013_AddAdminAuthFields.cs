using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjoCoreBackend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminAuthFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationToken",
                table: "CooperativeAdmins",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                table: "CooperativeAdmins",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ResetPasswordToken",
                table: "CooperativeAdmins",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetPasswordTokenExpiry",
                table: "CooperativeAdmins",
                type: "timestamp with time zone",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "EmailVerificationToken",
                table: "CooperativeAdmins");

            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                table: "CooperativeAdmins");

            migrationBuilder.DropColumn(
                name: "ResetPasswordToken",
                table: "CooperativeAdmins");

            migrationBuilder.DropColumn(
                name: "ResetPasswordTokenExpiry",
                table: "CooperativeAdmins");
        }
    }
}
