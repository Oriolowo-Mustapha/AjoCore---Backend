using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjoCoreBackend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminPasswordHashes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SystemAdmins",
                keyColumn: "Id",
                keyValue: new Guid("d8a8b132-23cf-4235-866c-333e213320c4"),
                column: "PasswordHash",
                value: "$2a$12$BjORNSCRSBhTUck456hyBuQz4dhMQMj/HcDnmOUxWiGMRARp9Qln6");

            migrationBuilder.UpdateData(
                table: "SystemAdmins",
                keyColumn: "Id",
                keyValue: new Guid("f827e8d6-444f-4a0b-8d14-b5ebc5344d57"),
                column: "PasswordHash",
                value: "$2a$12$q28FxVwEjqbz66XrZ8NfVu56dS8ZxAUPrVHRAwPLu4.Nakr/bJwm6");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
