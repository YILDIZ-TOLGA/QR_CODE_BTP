using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BTPSecure.Server.Migrations
{
    /// <inheritdoc />
    public partial class AjoutResetMotDePasse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "resets_motdepasse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UtilisateurId = table.Column<int>(type: "integer", nullable: false),
                    Token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateExpiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstUtilise = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resets_motdepasse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_resets_motdepasse_utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_resets_motdepasse_Token",
                table: "resets_motdepasse",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_resets_motdepasse_UtilisateurId",
                table: "resets_motdepasse",
                column: "UtilisateurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "resets_motdepasse");
        }
    }
}
