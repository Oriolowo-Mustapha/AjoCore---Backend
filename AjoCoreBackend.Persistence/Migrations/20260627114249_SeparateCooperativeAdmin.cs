using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjoCoreBackend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeparateCooperativeAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CooperativeAdmins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CooperativeAdmins", x => x.Id);
                });

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
                name: "Traders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Bvn = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResetPasswordToken = table.Column<string>(type: "text", nullable: true),
                    ResetPasswordTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    EmailVerificationToken = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Traders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CooperativeGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CooperativeAdminId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CooperativeGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CooperativeGroups_CooperativeAdmins_CooperativeAdminId",
                        column: x => x.CooperativeAdminId,
                        principalTable: "CooperativeAdmins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InboundTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TraderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TransactionReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboundTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InboundTransactions_Traders_TraderId",
                        column: x => x.TraderId,
                        principalTable: "Traders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CooperativeGroupMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CooperativeGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    TraderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CooperativeGroupMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CooperativeGroupMembers_CooperativeGroups_CooperativeGroupId",
                        column: x => x.CooperativeGroupId,
                        principalTable: "CooperativeGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CooperativeGroupMembers_Traders_TraderId",
                        column: x => x.TraderId,
                        principalTable: "Traders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    CooperativeGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingCycles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavingCycles_CooperativeGroups_CooperativeGroupId",
                        column: x => x.CooperativeGroupId,
                        principalTable: "CooperativeGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                    ApprovalStatus = table.Column<int>(type: "integer", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "RotationalSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SavingCycleId = table.Column<Guid>(type: "uuid", nullable: false),
                    SlotNumber = table.Column<int>(type: "integer", nullable: false),
                    EstimatedPayoutDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsAssigned = table.Column<bool>(type: "boolean", nullable: false),
                    AssignedMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RotationalSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RotationalSlots_SavingCycleMembers_AssignedMemberId",
                        column: x => x.AssignedMemberId,
                        principalTable: "SavingCycleMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RotationalSlots_SavingCycles_SavingCycleId",
                        column: x => x.SavingCycleId,
                        principalTable: "SavingCycles",
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
                name: "IX_CooperativeAdmins_Email",
                table: "CooperativeAdmins",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CooperativeGroupMembers_CooperativeGroupId_TraderId",
                table: "CooperativeGroupMembers",
                columns: new[] { "CooperativeGroupId", "TraderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CooperativeGroupMembers_TraderId",
                table: "CooperativeGroupMembers",
                column: "TraderId");

            migrationBuilder.CreateIndex(
                name: "IX_CooperativeGroups_CooperativeAdminId",
                table: "CooperativeGroups",
                column: "CooperativeAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_InboundTransactions_TraderId",
                table: "InboundTransactions",
                column: "TraderId");

            migrationBuilder.CreateIndex(
                name: "IX_InboundTransactions_TransactionReference",
                table: "InboundTransactions",
                column: "TransactionReference",
                unique: true);

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
                name: "IX_RotationalSlots_AssignedMemberId",
                table: "RotationalSlots",
                column: "AssignedMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_RotationalSlots_SavingCycleId_SlotNumber",
                table: "RotationalSlots",
                columns: new[] { "SavingCycleId", "SlotNumber" },
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_SavingCycles_CooperativeGroupId",
                table: "SavingCycles",
                column: "CooperativeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Traders_Email",
                table: "Traders",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContributionLedgers");

            migrationBuilder.DropTable(
                name: "CooperativeGroupMembers");

            migrationBuilder.DropTable(
                name: "InboundTransactions");

            migrationBuilder.DropTable(
                name: "PayoutLedgers");

            migrationBuilder.DropTable(
                name: "RotationalSlots");

            migrationBuilder.DropTable(
                name: "Traders");

            migrationBuilder.DropTable(
                name: "SavingCycleMembers");

            migrationBuilder.DropTable(
                name: "NombaVirtualAccounts");

            migrationBuilder.DropTable(
                name: "SavingCycles");

            migrationBuilder.DropTable(
                name: "CooperativeGroups");

            migrationBuilder.DropTable(
                name: "CooperativeAdmins");
        }
    }
}
