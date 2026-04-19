using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracklass.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthAndUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaCreacion",
                table: "Clases",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Fecha",
                table: "Clases",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaCreacion",
                table: "Alumnos",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            var adminId = new Guid("11111111-1111-1111-1111-111111111111");
            var adminHash = BCrypt.Net.BCrypt.HashPassword("admin123");

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Email", "PasswordHash", "Nombre", "FechaCreacion" },
                values: new object[] { adminId, "admin@tracklass.com", adminHash, "Administrador", DateTime.UtcNow }
            );

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioId",
                table: "Alumnos",
                type: "uuid",
                nullable: false,
                defaultValue: adminId);


            migrationBuilder.CreateIndex(
                name: "IX_Alumnos_UsuarioId",
                table: "Alumnos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Alumnos_Usuarios_UsuarioId",
                table: "Alumnos",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alumnos_Usuarios_UsuarioId",
                table: "Alumnos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Alumnos_UsuarioId",
                table: "Alumnos");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Alumnos");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaCreacion",
                table: "Clases",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Fecha",
                table: "Clases",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaCreacion",
                table: "Alumnos",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }
    }
}
