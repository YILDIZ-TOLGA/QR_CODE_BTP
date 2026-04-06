using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BTPSecure.Server.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "utilisateurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MotDePasseHash = table.Column<string>(type: "text", nullable: false),
                    Sel = table.Column<string>(type: "text", nullable: false),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Telephone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstActif = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utilisateurs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "entreprises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Adresse = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Siret = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PatronId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entreprises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_entreprises_utilisateurs_PatronId",
                        column: x => x.PatronId,
                        principalTable: "utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "codes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Valeur = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    TypeCode = table.Column<int>(type: "integer", nullable: false),
                    Statut = table.Column<int>(type: "integer", nullable: false),
                    NumeroCommande = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NomEntreprise = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Info = table.Column<string>(type: "text", nullable: true),
                    ListeMateriaux = table.Column<string>(type: "text", nullable: true),
                    DureeValidite = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateExpiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateValidation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PatronId = table.Column<int>(type: "integer", nullable: false),
                    SalarieId = table.Column<int>(type: "integer", nullable: false),
                    FournisseurId = table.Column<int>(type: "integer", nullable: true),
                    EntrepriseId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_codes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_codes_entreprises_EntrepriseId",
                        column: x => x.EntrepriseId,
                        principalTable: "entreprises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_codes_utilisateurs_FournisseurId",
                        column: x => x.FournisseurId,
                        principalTable: "utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_codes_utilisateurs_PatronId",
                        column: x => x.PatronId,
                        principalTable: "utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_codes_utilisateurs_SalarieId",
                        column: x => x.SalarieId,
                        principalTable: "utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "salaries_entreprises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SalarieId = table.Column<int>(type: "integer", nullable: false),
                    EntrepriseId = table.Column<int>(type: "integer", nullable: false),
                    DateAjout = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstActif = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_salaries_entreprises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_salaries_entreprises_entreprises_EntrepriseId",
                        column: x => x.EntrepriseId,
                        principalTable: "entreprises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_salaries_entreprises_utilisateurs_SalarieId",
                        column: x => x.SalarieId,
                        principalTable: "utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_codes_EntrepriseId",
                table: "codes",
                column: "EntrepriseId");

            migrationBuilder.CreateIndex(
                name: "IX_codes_FournisseurId",
                table: "codes",
                column: "FournisseurId");

            migrationBuilder.CreateIndex(
                name: "IX_codes_PatronId",
                table: "codes",
                column: "PatronId");

            migrationBuilder.CreateIndex(
                name: "IX_codes_SalarieId",
                table: "codes",
                column: "SalarieId");

            migrationBuilder.CreateIndex(
                name: "IX_codes_Valeur",
                table: "codes",
                column: "Valeur",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_entreprises_PatronId",
                table: "entreprises",
                column: "PatronId");

            migrationBuilder.CreateIndex(
                name: "IX_salaries_entreprises_EntrepriseId",
                table: "salaries_entreprises",
                column: "EntrepriseId");

            migrationBuilder.CreateIndex(
                name: "IX_salaries_entreprises_SalarieId",
                table: "salaries_entreprises",
                column: "SalarieId");

            migrationBuilder.CreateIndex(
                name: "IX_utilisateurs_Email",
                table: "utilisateurs",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "codes");

            migrationBuilder.DropTable(
                name: "salaries_entreprises");

            migrationBuilder.DropTable(
                name: "entreprises");

            migrationBuilder.DropTable(
                name: "utilisateurs");
        }
    }
}
