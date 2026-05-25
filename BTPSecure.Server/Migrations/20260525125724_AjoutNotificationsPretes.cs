using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTPSecure.Server.Migrations
{
    /// <inheritdoc />
    public partial class AjoutNotificationsPretes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DatePrete",
                table: "codes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EstPrete",
                table: "codes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatePrete",
                table: "codes");

            migrationBuilder.DropColumn(
                name: "EstPrete",
                table: "codes");
        }
    }
}
