using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlTaxisApp.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposContactolicencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactoApellidos",
                table: "Conductores",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactoCorreo",
                table: "Conductores",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactoNombre",
                table: "Conductores",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactoTelefono",
                table: "Conductores",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenciaAtrasUrl",
                table: "Conductores",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenciaFrenteUrl",
                table: "Conductores",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactoApellidos",
                table: "Conductores");

            migrationBuilder.DropColumn(
                name: "ContactoCorreo",
                table: "Conductores");

            migrationBuilder.DropColumn(
                name: "ContactoNombre",
                table: "Conductores");

            migrationBuilder.DropColumn(
                name: "ContactoTelefono",
                table: "Conductores");

            migrationBuilder.DropColumn(
                name: "LicenciaAtrasUrl",
                table: "Conductores");

            migrationBuilder.DropColumn(
                name: "LicenciaFrenteUrl",
                table: "Conductores");
        }
    }
}
