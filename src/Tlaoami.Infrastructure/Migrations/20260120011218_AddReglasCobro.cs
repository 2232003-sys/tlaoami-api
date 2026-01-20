using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlaoami.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReglasCobro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConceptosCobro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Clave = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Periodicidad = table.Column<string>(type: "text", nullable: true),
                    RequiereCFDI = table.Column<bool>(type: "boolean", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceptosCobro", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReglasCobro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CicloId = table.Column<Guid>(type: "uuid", nullable: false),
                    Grado = table.Column<int>(type: "integer", nullable: true),
                    Turno = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConceptoCobroId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipoGeneracion = table.Column<string>(type: "text", nullable: false),
                    DiaCorte = table.Column<int>(type: "integer", nullable: true),
                    MontoBase = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglasCobro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReglasCobro_CiclosEscolares_CicloId",
                        column: x => x.CicloId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReglasCobro_ConceptosCobro_ConceptoCobroId",
                        column: x => x.ConceptoCobroId,
                        principalTable: "ConceptosCobro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConceptosCobro_Clave",
                table: "ConceptosCobro",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReglasCobro_ConceptoCobroId",
                table: "ReglasCobro",
                column: "ConceptoCobroId");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasCobro_Unique_Logico",
                table: "ReglasCobro",
                columns: new[] { "CicloId", "Grado", "Turno", "ConceptoCobroId", "TipoGeneracion" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReglasCobro");

            migrationBuilder.DropTable(
                name: "ConceptosCobro");
        }
    }
}
