using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTPSecure.Server.Migrations
{
    /// <inheritdoc />
    public partial class AjoutCreateurCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreateurId",
                table: "codes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_codes_CreateurId",
                table: "codes",
                column: "CreateurId");

            migrationBuilder.AddForeignKey(
                name: "FK_codes_utilisateurs_CreateurId",
                table: "codes",
                column: "CreateurId",
                principalTable: "utilisateurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_codes_utilisateurs_CreateurId",
                table: "codes");

            migrationBuilder.DropIndex(
                name: "IX_codes_CreateurId",
                table: "codes");

            migrationBuilder.DropColumn(
                name: "CreateurId",
                table: "codes");
        }
    }
}
