using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlaoami.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMovimientoConciliacionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MovimientosBancarios_HashMovimiento",
                table: "MovimientosBancarios");

            migrationBuilder.CreateTable(
                name: "MovimientosConciliacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MovimientoBancarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FacturaId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaConciliacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Comentario = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosConciliacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosConciliacion_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosConciliacion_Facturas_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "Facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosConciliacion_MovimientosBancarios_MovimientoBancarioId",
                        column: x => x.MovimientoBancarioId,
                        principalTable: "MovimientosBancarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosConciliacion_AlumnoId",
                table: "MovimientosConciliacion",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosConciliacion_FacturaId",
                table: "MovimientosConciliacion",
                column: "FacturaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosConciliacion_MovimientoBancarioId",
                table: "MovimientosConciliacion",
                column: "MovimientoBancarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimientosConciliacion");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosBancarios_HashMovimiento",
                table: "MovimientosBancarios",
                column: "HashMovimiento",
                unique: true);
        }
    }
}
