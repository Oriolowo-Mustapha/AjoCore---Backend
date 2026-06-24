using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjoCoreBackend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NombaVirtualAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubAccountId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccountNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AccountName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NombaVirtualAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavingCycles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CycleType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ContributionAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IntervalDays = table.Column<int>(type: "integer", nullable: false),
                    NombaSubAccountId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingCycles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavingCycleMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SavingCycleId = table.Column<Guid>(type: "uuid", nullable: false),
                    NombaVirtualAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayoutOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingCycleMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavingCycleMembers_NombaVirtualAccounts_NombaVirtualAccount~",
                        column: x => x.NombaVirtualAccountId,
                        principalTable: "NombaVirtualAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SavingCycleMembers_SavingCycles_SavingCycleId",
                        column: x => x.SavingCycleId,
                        principalTable: "SavingCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContributionLedgers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SavingCycleMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NombaWebhookRequestId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContributionLedgers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContributionLedgers_SavingCycleMembers_SavingCycleMemberId",
                        column: x => x.SavingCycleMemberId,
                        principalTable: "SavingCycleMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayoutLedgers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SavingCycleMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MerchantTxRef = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PayoutDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutLedgers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayoutLedgers_SavingCycleMembers_SavingCycleMemberId",
                        column: x => x.SavingCycleMemberId,
                        principalTable: "SavingCycleMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContributionLedgers_NombaWebhookRequestId",
                table: "ContributionLedgers",
                column: "NombaWebhookRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContributionLedgers_SavingCycleMemberId",
                table: "ContributionLedgers",
                column: "SavingCycleMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutLedgers_MerchantTxRef",
                table: "PayoutLedgers",
                column: "MerchantTxRef",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayoutLedgers_SavingCycleMemberId",
                table: "PayoutLedgers",
                column: "SavingCycleMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingCycleMembers_NombaVirtualAccountId",
                table: "SavingCycleMembers",
                column: "NombaVirtualAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavingCycleMembers_SavingCycleId_PayoutOrder",
                table: "SavingCycleMembers",
                columns: new[] { "SavingCycleId", "PayoutOrder" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContributionLedgers");

            migrationBuilder.DropTable(
                name: "PayoutLedgers");

            migrationBuilder.DropTable(
                name: "SavingCycleMembers");

            migrationBuilder.DropTable(
                name: "NombaVirtualAccounts");

            migrationBuilder.DropTable(
                name: "SavingCycles");
        }
    }
}
