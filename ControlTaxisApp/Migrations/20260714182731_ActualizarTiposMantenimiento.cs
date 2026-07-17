using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ControlTaxisApp.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarTiposMantenimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TiposMantenimiento",
                keyColumn: "Id",
                keyValue: 3,
                column: "Nombre",
                value: "Suspensión");

            migrationBuilder.UpdateData(
                table: "TiposMantenimiento",
                keyColumn: "Id",
                keyValue: 4,
                column: "Nombre",
                value: "Sistema Eléctrico");

            migrationBuilder.UpdateData(
                table: "TiposMantenimiento",
                keyColumn: "Id",
                keyValue: 5,
                column: "Nombre",
                value: "Cambio de Aceite");

            migrationBuilder.UpdateData(
                table: "TiposMantenimiento",
                keyColumn: "Id",
                keyValue: 6,
                column: "Nombre",
                value: "Frenos");

            migrationBuilder.UpdateData(
                table: "TiposMantenimiento",
                keyColumn: "Id",
                keyValue: 7,
                column: "Nombre",
                value: "Cambio Correa Rep.");

            migrationBuilder.UpdateData(
                table: "TiposMantenimiento",
                keyColumn: "Id",
                keyValue: 8,
                column: "Nombre",
                value: "Cambio Correa Dite");

            migrationBuilder.UpdateData(
                table: "TiposMantenimiento",
                keyColumn: "Id",
                keyValue: 9,
                column: "Nombre",
                value: "Alternador");

            migrationBuilder.InsertData(
                table: "TiposMantenimiento",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 10, "Rayador" },
                    { 11, "Closh" },
                    { 12, "Pintura" },
                    { 13, "Llantas" },
                    { 14, "Batería" },
                    { 15, "Otro" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TiposMantenimiento",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "TiposMantenimiento",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "TiposMantenimiento",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "TiposMantenimiento",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "TiposMantenimiento",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "TiposMantenimiento",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.UpdateData(
                table: "TiposMantenimiento",
                keyColumn: "Id",
                keyValue: 3,
                column: "Nombre",
                value: "Frenos");

            migrationBuilder.UpdateData(
                table: "TiposMantenimiento",
                keyColumn: "Id",
                keyValue: 4,
                column: "Nombre",
                value: "Suspensión");

            migrationBuilder.UpdateData(
                table: "TiposMantenimiento",
                keyColumn: "Id",
                keyValue: 5,
                column: "Nombre",
                value: "Sistema Eléctrico");

            migrationBuilder.UpdateData(
                table: "TiposMantenimiento",
                keyColumn: "Id",
                keyValue: 6,
                column: "Nombre",
                value: "Llantas");

            migrationBuilder.UpdateData(
                table: "TiposMantenimiento",
                keyColumn: "Id",
                keyValue: 7,
                column: "Nombre",
                value: "Cambio de Aceite");

            migrationBuilder.UpdateData(
                table: "TiposMantenimiento",
                keyColumn: "Id",
                keyValue: 8,
                column: "Nombre",
                value: "Aire Acondicionado");

            migrationBuilder.UpdateData(
                table: "TiposMantenimiento",
                keyColumn: "Id",
                keyValue: 9,
                column: "Nombre",
                value: "Otro");
        }
    }
}
