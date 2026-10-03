using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BTPSecure.Server.Migrations
{
    /// <inheritdoc />
    public partial class AjoutApporteurAffaire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeParrainage",
                table: "utilisateurs",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "parrainages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApporteurId = table.Column<int>(type: "integer", nullable: false),
                    FilleulId = table.Column<int>(type: "integer", nullable: false),
                    CodeUtilise = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    DateInscription = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Statut = table.Column<int>(type: "integer", nullable: false),
                    MontantCommission = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    DateValidation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidateurId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parrainages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_parrainages_utilisateurs_ApporteurId",
                        column: x => x.ApporteurId,
                        principalTable: "utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_parrainages_utilisateurs_FilleulId",
                        column: x => x.FilleulId,
                        principalTable: "utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_parrainages_utilisateurs_ValidateurId",
                        column: x => x.ValidateurId,
                        principalTable: "utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_utilisateurs_CodeParrainage",
                table: "utilisateurs",
                column: "CodeParrainage",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parrainages_ApporteurId_Statut",
                table: "parrainages",
                columns: new[] { "ApporteurId", "Statut" });

            migrationBuilder.CreateIndex(
                name: "IX_parrainages_FilleulId",
                table: "parrainages",
                column: "FilleulId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parrainages_ValidateurId",
                table: "parrainages",
                column: "ValidateurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parrainages");

            migrationBuilder.DropIndex(
                name: "IX_utilisateurs_CodeParrainage",
                table: "utilisateurs");

            migrationBuilder.DropColumn(
                name: "CodeParrainage",
                table: "utilisateurs");
        }
    }
}
