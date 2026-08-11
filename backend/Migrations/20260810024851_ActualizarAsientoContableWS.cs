using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaDeCompras.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarAsientoContableWS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CuentaContable",
                table: "AsientosContables");

            migrationBuilder.RenameColumn(
                name: "TipoMovimiento",
                table: "AsientosContables",
                newName: "CuentaDebitoId");

            migrationBuilder.RenameColumn(
                name: "TipoInventarioId",
                table: "AsientosContables",
                newName: "CuentaCreditoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CuentaDebitoId",
                table: "AsientosContables",
                newName: "TipoMovimiento");

            migrationBuilder.RenameColumn(
                name: "CuentaCreditoId",
                table: "AsientosContables",
                newName: "TipoInventarioId");

            migrationBuilder.AddColumn<string>(
                name: "CuentaContable",
                table: "AsientosContables",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }
    }
}
