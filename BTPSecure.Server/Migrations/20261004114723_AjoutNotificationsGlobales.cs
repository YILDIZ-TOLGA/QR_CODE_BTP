using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BTPSecure.Server.Migrations
{
    /// <inheritdoc />
    public partial class AjoutNotificationsGlobales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notifications_globales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Severite = table.Column<int>(type: "integer", nullable: false),
                    RolesCibles = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DateDebut = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateurId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications_globales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notifications_globales_utilisateurs_CreateurId",
                        column: x => x.CreateurId,
                        principalTable: "utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications_globales_vues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NotificationGlobaleId = table.Column<int>(type: "integer", nullable: false),
                    UtilisateurId = table.Column<int>(type: "integer", nullable: false),
                    DateVue = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications_globales_vues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notifications_globales_vues_notifications_globales_Notifica~",
                        column: x => x.NotificationGlobaleId,
                        principalTable: "notifications_globales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notifications_globales_vues_utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_globales_CreateurId",
                table: "notifications_globales",
                column: "CreateurId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_globales_EstActive_DateDebut_DateFin",
                table: "notifications_globales",
                columns: new[] { "EstActive", "DateDebut", "DateFin" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_globales_vues_NotificationGlobaleId_Utilisate~",
                table: "notifications_globales_vues",
                columns: new[] { "NotificationGlobaleId", "UtilisateurId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_globales_vues_UtilisateurId",
                table: "notifications_globales_vues",
                column: "UtilisateurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notifications_globales_vues");

            migrationBuilder.DropTable(
                name: "notifications_globales");
        }
    }
}
