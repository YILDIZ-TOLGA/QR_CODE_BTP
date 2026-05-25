using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTPSecure.Server.Migrations
{
    /// <inheritdoc />
    public partial class AjoutVerificationEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailVerifie",
                table: "utilisateurs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TokenVerification",
                table: "utilisateurs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenVerificationExpiration",
                table: "utilisateurs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_utilisateurs_TokenVerification",
                table: "utilisateurs",
                column: "TokenVerification");

            // Marquer tous les comptes existants comme vérifiés pour ne pas les bloquer
            migrationBuilder.Sql(@"UPDATE utilisateurs SET ""EmailVerifie"" = true;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_utilisateurs_TokenVerification",
                table: "utilisateurs");

            migrationBuilder.DropColumn(
                name: "EmailVerifie",
                table: "utilisateurs");

            migrationBuilder.DropColumn(
                name: "TokenVerification",
                table: "utilisateurs");

            migrationBuilder.DropColumn(
                name: "TokenVerificationExpiration",
                table: "utilisateurs");
        }
    }
}
