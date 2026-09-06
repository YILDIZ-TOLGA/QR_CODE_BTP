using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BTPSecure.Server.Migrations
{
    /// <inheritdoc />
    public partial class AjoutHistoriqueValidations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "validations_codes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CodeId = table.Column<int>(type: "integer", nullable: false),
                    EntrepriseId = table.Column<int>(type: "integer", nullable: false),
                    PorteurId = table.Column<int>(type: "integer", nullable: true),
                    EmailTiers = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ValidateurId = table.Column<int>(type: "integer", nullable: false),
                    DateValidation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValeurUtilisee = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    NumeroCommande = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AchatsSupplementaires = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    EstPermanent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_validations_codes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_validations_codes_codes_CodeId",
                        column: x => x.CodeId,
                        principalTable: "codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_validations_codes_utilisateurs_PorteurId",
                        column: x => x.PorteurId,
                        principalTable: "utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_validations_codes_utilisateurs_ValidateurId",
                        column: x => x.ValidateurId,
                        principalTable: "utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_validations_codes_CodeId",
                table: "validations_codes",
                column: "CodeId");

            migrationBuilder.CreateIndex(
                name: "IX_validations_codes_EntrepriseId_DateValidation",
                table: "validations_codes",
                columns: new[] { "EntrepriseId", "DateValidation" });

            migrationBuilder.CreateIndex(
                name: "IX_validations_codes_PorteurId",
                table: "validations_codes",
                column: "PorteurId");

            migrationBuilder.CreateIndex(
                name: "IX_validations_codes_ValidateurId",
                table: "validations_codes",
                column: "ValidateurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "validations_codes");
        }
    }
}
