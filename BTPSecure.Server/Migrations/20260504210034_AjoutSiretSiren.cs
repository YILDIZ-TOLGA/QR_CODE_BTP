using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTPSecure.Server.Migrations
{
    /// <inheritdoc />
    public partial class AjoutSiretSiren : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Siren",
                table: "utilisateurs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Siret",
                table: "utilisateurs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Siren",
                table: "utilisateurs");

            migrationBuilder.DropColumn(
                name: "Siret",
                table: "utilisateurs");
        }
    }
}
