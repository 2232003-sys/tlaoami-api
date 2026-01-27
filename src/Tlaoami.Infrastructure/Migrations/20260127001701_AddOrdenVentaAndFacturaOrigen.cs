using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlaoami.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdenVentaAndFacturaOrigen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrigenId",
                table: "Facturas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrigenTipo",
                table: "Facturas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrdenesVenta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estatus = table.Column<string>(type: "text", nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Notas = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FacturaId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConfirmadaAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenesVenta_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdenesVentaLineas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdenVentaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesVentaLineas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenesVentaLineas_ConceptosCobro_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "ConceptosCobro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesVentaLineas_OrdenesVenta_OrdenVentaId",
                        column: x => x.OrdenVentaId,
                        principalTable: "OrdenesVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesVenta_AlumnoId",
                table: "OrdenesVenta",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesVenta_Estatus",
                table: "OrdenesVenta",
                column: "Estatus");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesVenta_Fecha",
                table: "OrdenesVenta",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesVentaLineas_OrdenVentaId",
                table: "OrdenesVentaLineas",
                column: "OrdenVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesVentaLineas_ProductoId",
                table: "OrdenesVentaLineas",
                column: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrdenesVentaLineas");

            migrationBuilder.DropTable(
                name: "OrdenesVenta");

            migrationBuilder.DropColumn(
                name: "OrigenId",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "OrigenTipo",
                table: "Facturas");
        }
    }
}
