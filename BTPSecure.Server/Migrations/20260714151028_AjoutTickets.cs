using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BTPSecure.Server.Migrations
{
    /// <inheritdoc />
    public partial class AjoutTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExpediteurId = table.Column<int>(type: "integer", nullable: false),
                    DestinataireId = table.Column<int>(type: "integer", nullable: true),
                    EmailDestinataire = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Sujet = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    PieceJointe = table.Column<byte[]>(type: "bytea", nullable: true),
                    NomPieceJointe = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TypePieceJointe = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstLu = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tickets_utilisateurs_DestinataireId",
                        column: x => x.DestinataireId,
                        principalTable: "utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tickets_utilisateurs_ExpediteurId",
                        column: x => x.ExpediteurId,
                        principalTable: "utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tickets_DateCreation",
                table: "tickets",
                column: "DateCreation");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_DestinataireId",
                table: "tickets",
                column: "DestinataireId");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_ExpediteurId",
                table: "tickets",
                column: "ExpediteurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tickets");
        }
    }
}
