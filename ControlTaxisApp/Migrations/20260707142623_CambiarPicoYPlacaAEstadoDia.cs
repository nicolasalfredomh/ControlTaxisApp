using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlTaxisApp.Migrations
{
    /// <inheritdoc />
    public partial class CambiarPicoYPlacaAEstadoDia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstadoDia",
                table: "LiquidacionesDiarias",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstadoDia",
                table: "LiquidacionesDiarias");
        }
    }
}
