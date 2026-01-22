using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlaoami.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReceptorFiscalAndFacturaFiscal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FacturasFiscales",
                columns: table => new
                {
                    FacturaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Proveedor = table.Column<string>(type: "text", nullable: false),
                    EstadoTimbrado = table.Column<string>(type: "text", nullable: false),
                    CfdiUuid = table.Column<string>(type: "text", nullable: true),
                    CfdiXmlBase64 = table.Column<string>(type: "text", nullable: true),
                    CfdiPdfBase64 = table.Column<string>(type: "text", nullable: true),
                    TimbradoAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorTimbrado = table.Column<string>(type: "text", nullable: true),
                    UsoCfdi = table.Column<string>(type: "text", nullable: true),
                    MetodoPago = table.Column<string>(type: "text", nullable: true),
                    FormaPago = table.Column<string>(type: "text", nullable: true),
                    ReceptorRfcSnapshot = table.Column<string>(type: "text", nullable: true),
                    ReceptorNombreSnapshot = table.Column<string>(type: "text", nullable: true),
                    ReceptorCodigoPostalSnapshot = table.Column<string>(type: "text", nullable: true),
                    ReceptorRegimenSnapshot = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturasFiscales", x => x.FacturaId);
                    table.ForeignKey(
                        name: "FK_FacturasFiscales_Facturas_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "Facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReceptoresFiscales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rfc = table.Column<string>(type: "text", nullable: false),
                    NombreFiscal = table.Column<string>(type: "text", nullable: false),
                    CodigoPostalFiscal = table.Column<string>(type: "text", nullable: false),
                    RegimenFiscal = table.Column<string>(type: "text", nullable: false),
                    UsoCfdiDefault = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceptoresFiscales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceptoresFiscales_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FacturasFiscales_CfdiUuid",
                table: "FacturasFiscales",
                column: "CfdiUuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceptoresFiscales_AlumnoId",
                table: "ReceptoresFiscales",
                column: "AlumnoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceptoresFiscales_Rfc",
                table: "ReceptoresFiscales",
                column: "Rfc",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FacturasFiscales");

            migrationBuilder.DropTable(
                name: "ReceptoresFiscales");
        }
    }
}
