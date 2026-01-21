using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlaoami.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMovimientoConciliacionUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MovimientosConciliacion_MovimientoBancarioId",
                table: "MovimientosConciliacion");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosConciliacion_MovimientoBancarioId",
                table: "MovimientosConciliacion",
                column: "MovimientoBancarioId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MovimientosConciliacion_MovimientoBancarioId",
                table: "MovimientosConciliacion");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosConciliacion_MovimientoBancarioId",
                table: "MovimientosConciliacion",
                column: "MovimientoBancarioId");
        }
    }
}
