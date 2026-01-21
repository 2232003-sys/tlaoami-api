using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlaoami.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddColegiaturasMensuales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Facturas_AlumnoId",
                table: "Facturas");

            migrationBuilder.AddColumn<Guid>(
                name: "ConceptoCobroId",
                table: "Facturas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Periodo",
                table: "Facturas",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReciboEmitidoAtUtc",
                table: "Facturas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReciboFolio",
                table: "Facturas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoDocumento",
                table: "Facturas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "BecasAlumno",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CicloId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BecasAlumno", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BecasAlumno_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BecasAlumno_CiclosEscolares_CicloId",
                        column: x => x.CicloId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FacturaLineas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacturaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConceptoCobroId = table.Column<Guid>(type: "uuid", nullable: true),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Descuento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Impuesto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturaLineas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacturaLineas_ConceptosCobro_ConceptoCobroId",
                        column: x => x.ConceptoCobroId,
                        principalTable: "ConceptosCobro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FacturaLineas_Facturas_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "Facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReglasColegiatura",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CicloId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrupoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Grado = table.Column<int>(type: "integer", nullable: true),
                    Turno = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConceptoCobroId = table.Column<Guid>(type: "uuid", nullable: false),
                    MontoBase = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiaVencimiento = table.Column<int>(type: "integer", nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglasColegiatura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReglasColegiatura_CiclosEscolares_CicloId",
                        column: x => x.CicloId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReglasColegiatura_ConceptosCobro_ConceptoCobroId",
                        column: x => x.ConceptoCobroId,
                        principalTable: "ConceptosCobro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReglasColegiatura_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ReglasRecargo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CicloId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConceptoCobroId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiasGracia = table.Column<int>(type: "integer", nullable: false),
                    Porcentaje = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglasRecargo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReglasRecargo_CiclosEscolares_CicloId",
                        column: x => x.CicloId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReglasRecargo_ConceptosCobro_ConceptoCobroId",
                        column: x => x.ConceptoCobroId,
                        principalTable: "ConceptosCobro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Factura_Alumno_Periodo_Concepto",
                table: "Facturas",
                columns: new[] { "AlumnoId", "Periodo", "ConceptoCobroId" },
                unique: true,
                filter: "\"Estado\" <> 'Cancelada'");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_ConceptoCobroId",
                table: "Facturas",
                column: "ConceptoCobroId");

            migrationBuilder.CreateIndex(
                name: "IX_BecasAlumno_AlumnoId_CicloId",
                table: "BecasAlumno",
                columns: new[] { "AlumnoId", "CicloId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BecasAlumno_CicloId",
                table: "BecasAlumno",
                column: "CicloId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaLineas_ConceptoCobroId",
                table: "FacturaLineas",
                column: "ConceptoCobroId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaLineas_FacturaId_ConceptoCobroId",
                table: "FacturaLineas",
                columns: new[] { "FacturaId", "ConceptoCobroId" });

            migrationBuilder.CreateIndex(
                name: "IX_ReglaColegiatura_Unique",
                table: "ReglasColegiatura",
                columns: new[] { "CicloId", "GrupoId", "Grado", "Turno", "ConceptoCobroId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReglasColegiatura_ConceptoCobroId",
                table: "ReglasColegiatura",
                column: "ConceptoCobroId");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasColegiatura_GrupoId",
                table: "ReglasColegiatura",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasRecargo_CicloId",
                table: "ReglasRecargo",
                column: "CicloId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReglasRecargo_ConceptoCobroId",
                table: "ReglasRecargo",
                column: "ConceptoCobroId");

            migrationBuilder.AddForeignKey(
                name: "FK_Facturas_ConceptosCobro_ConceptoCobroId",
                table: "Facturas",
                column: "ConceptoCobroId",
                principalTable: "ConceptosCobro",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Facturas_ConceptosCobro_ConceptoCobroId",
                table: "Facturas");

            migrationBuilder.DropTable(
                name: "BecasAlumno");

            migrationBuilder.DropTable(
                name: "FacturaLineas");

            migrationBuilder.DropTable(
                name: "ReglasColegiatura");

            migrationBuilder.DropTable(
                name: "ReglasRecargo");

            migrationBuilder.DropIndex(
                name: "IX_Factura_Alumno_Periodo_Concepto",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_ConceptoCobroId",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "ConceptoCobroId",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "Periodo",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "ReciboEmitidoAtUtc",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "ReciboFolio",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "TipoDocumento",
                table: "Facturas");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_AlumnoId",
                table: "Facturas",
                column: "AlumnoId");
        }
    }
}
