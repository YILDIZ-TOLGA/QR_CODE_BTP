using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BTPSecure.Server.Migrations
{
    /// <inheritdoc />
    public partial class AjoutFournisseurContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FournisseurContactId",
                table: "codes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "codes",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "fournisseurs_contacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatronId = table.Column<int>(type: "integer", nullable: false),
                    NomEntreprise = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Siret = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    Siren = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fournisseurs_contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fournisseurs_contacts_utilisateurs_PatronId",
                        column: x => x.PatronId,
                        principalTable: "utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_codes_FournisseurContactId",
                table: "codes",
                column: "FournisseurContactId");

            migrationBuilder.CreateIndex(
                name: "IX_fournisseurs_contacts_PatronId_Siret",
                table: "fournisseurs_contacts",
                columns: new[] { "PatronId", "Siret" });

            migrationBuilder.AddForeignKey(
                name: "FK_codes_fournisseurs_contacts_FournisseurContactId",
                table: "codes",
                column: "FournisseurContactId",
                principalTable: "fournisseurs_contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_codes_fournisseurs_contacts_FournisseurContactId",
                table: "codes");

            migrationBuilder.DropTable(
                name: "fournisseurs_contacts");

            migrationBuilder.DropIndex(
                name: "IX_codes_FournisseurContactId",
                table: "codes");

            migrationBuilder.DropColumn(
                name: "FournisseurContactId",
                table: "codes");

            migrationBuilder.DropColumn(
                name: "Reference",
                table: "codes");
        }
    }
}
