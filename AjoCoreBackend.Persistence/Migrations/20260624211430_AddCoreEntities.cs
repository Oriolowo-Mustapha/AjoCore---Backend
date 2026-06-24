using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjoCoreBackend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCoreEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CooperativeGroupId",
                table: "SavingCycles",
                type: "uuid",
                nullable: true);

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

            migrationBuilder.CreateTable(
                name: "Traders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Bvn = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AdminTraderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CooperativeGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CooperativeGroups_Traders_AdminTraderId",
                        column: x => x.AdminTraderId,
                        principalTable: "Traders",
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

            migrationBuilder.CreateIndex(
                name: "IX_SavingCycles_CooperativeGroupId",
                table: "SavingCycles",
                column: "CooperativeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CooperativeGroups_AdminTraderId",
                table: "CooperativeGroups",
                column: "AdminTraderId");

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
                name: "IX_RotationalSlots_AssignedMemberId",
                table: "RotationalSlots",
                column: "AssignedMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_RotationalSlots_SavingCycleId_SlotNumber",
                table: "RotationalSlots",
                columns: new[] { "SavingCycleId", "SlotNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Traders_Email",
                table: "Traders",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SavingCycles_CooperativeGroups_CooperativeGroupId",
                table: "SavingCycles",
                column: "CooperativeGroupId",
                principalTable: "CooperativeGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SavingCycles_CooperativeGroups_CooperativeGroupId",
                table: "SavingCycles");

            migrationBuilder.DropTable(
                name: "CooperativeGroups");

            migrationBuilder.DropTable(
                name: "InboundTransactions");

            migrationBuilder.DropTable(
                name: "RotationalSlots");

            migrationBuilder.DropTable(
                name: "Traders");

            migrationBuilder.DropIndex(
                name: "IX_SavingCycles_CooperativeGroupId",
                table: "SavingCycles");

            migrationBuilder.DropColumn(
                name: "CooperativeGroupId",
                table: "SavingCycles");
        }
    }
}
