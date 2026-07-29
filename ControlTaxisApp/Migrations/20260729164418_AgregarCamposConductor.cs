using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlTaxisApp.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposConductor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Apellidos",
                table: "Conductores",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Conductores",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FotoUrl",
                table: "Conductores",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Conductores",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VehiculoId",
                table: "Conductores",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conductores_VehiculoId",
                table: "Conductores",
                column: "VehiculoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conductores_Vehiculos_VehiculoId",
                table: "Conductores",
                column: "VehiculoId",
                principalTable: "Vehiculos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conductores_Vehiculos_VehiculoId",
                table: "Conductores");

            migrationBuilder.DropIndex(
                name: "IX_Conductores_VehiculoId",
                table: "Conductores");

            migrationBuilder.DropColumn(
                name: "Apellidos",
                table: "Conductores");

            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Conductores");

            migrationBuilder.DropColumn(
                name: "FotoUrl",
                table: "Conductores");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Conductores");

            migrationBuilder.DropColumn(
                name: "VehiculoId",
                table: "Conductores");
        }
    }
}
