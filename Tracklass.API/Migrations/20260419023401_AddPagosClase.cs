using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracklass.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPagosClase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaPago",
                table: "Clases",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Pagada",
                table: "Clases",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaPago",
                table: "Clases");

            migrationBuilder.DropColumn(
                name: "Pagada",
                table: "Clases");
        }
    }
}
