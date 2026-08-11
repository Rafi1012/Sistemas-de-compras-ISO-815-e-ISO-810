using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaDeCompras.Migrations
{
    /// <inheritdoc />
    public partial class AgregarNumeroAsientoContabilidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Asiento",
                table: "AsientosContables",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Asiento",
                table: "AsientosContables");
        }
    }
}
