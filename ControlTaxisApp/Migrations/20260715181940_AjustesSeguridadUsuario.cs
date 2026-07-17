using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlTaxisApp.Migrations
{
    /// <inheritdoc />
    public partial class AjustesSeguridadUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsuarioId",
                table: "Vehiculos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Vehiculos");
        }
    }
}
