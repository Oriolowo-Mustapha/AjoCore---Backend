using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjoCoreBackend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncRailwayModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SystemAdmins",
                keyColumn: "Id",
                keyValue: new Guid("d8a8b132-23cf-4235-866c-333e213320c4"),
                column: "PasswordHash",
                value: "HASH_PLACEHOLDER");

            migrationBuilder.UpdateData(
                table: "SystemAdmins",
                keyColumn: "Id",
                keyValue: new Guid("f827e8d6-444f-4a0b-8d14-b5ebc5344d57"),
                column: "PasswordHash",
                value: "HASH_PLACEHOLDER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SystemAdmins",
                keyColumn: "Id",
                keyValue: new Guid("d8a8b132-23cf-4235-866c-333e213320c4"),
                column: "PasswordHash",
                value: "$2a$12$smqy6Q9xGlXerUxW.AV1YuKeNBCsTwVfap/uqw9EvmnsGsWRq4oTi");

            migrationBuilder.UpdateData(
                table: "SystemAdmins",
                keyColumn: "Id",
                keyValue: new Guid("f827e8d6-444f-4a0b-8d14-b5ebc5344d57"),
                column: "PasswordHash",
                value: "$2a$12$5TN4fUTIeV2KyeUdA0geAeqaQkuhbB2M9q.plC3n48PBLm3fx/ECS");
        }
    }
}
