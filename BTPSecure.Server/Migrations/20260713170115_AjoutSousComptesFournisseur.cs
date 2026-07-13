using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTPSecure.Server.Migrations
{
    /// <inheritdoc />
    public partial class AjoutSousComptesFournisseur : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentFournisseurId",
                table: "utilisateurs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_utilisateurs_ParentFournisseurId",
                table: "utilisateurs",
                column: "ParentFournisseurId");

            migrationBuilder.AddForeignKey(
                name: "FK_utilisateurs_utilisateurs_ParentFournisseurId",
                table: "utilisateurs",
                column: "ParentFournisseurId",
                principalTable: "utilisateurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_utilisateurs_utilisateurs_ParentFournisseurId",
                table: "utilisateurs");

            migrationBuilder.DropIndex(
                name: "IX_utilisateurs_ParentFournisseurId",
                table: "utilisateurs");

            migrationBuilder.DropColumn(
                name: "ParentFournisseurId",
                table: "utilisateurs");
        }
    }
}
