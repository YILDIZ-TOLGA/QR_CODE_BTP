using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTPSecure.Server.Migrations
{
    /// <inheritdoc />
    public partial class AjoutSessionUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "utilisateurs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "utilisateurs");
        }
    }
}
