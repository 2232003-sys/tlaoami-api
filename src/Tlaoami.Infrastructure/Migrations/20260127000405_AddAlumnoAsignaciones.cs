using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlaoami.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlumnoAsignaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlumnoAsignaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConceptoCobroId = table.Column<Guid>(type: "uuid", nullable: false),
                    CicloId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MontoOverride = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlumnoAsignaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlumnoAsignaciones_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlumnoAsignaciones_CiclosEscolares_CicloId",
                        column: x => x.CicloId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlumnoAsignaciones_ConceptosCobro_ConceptoCobroId",
                        column: x => x.ConceptoCobroId,
                        principalTable: "ConceptosCobro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlumnoAsignaciones_AlumnoId_Activo",
                table: "AlumnoAsignaciones",
                columns: new[] { "AlumnoId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_AlumnoAsignaciones_AlumnoId_CicloId",
                table: "AlumnoAsignaciones",
                columns: new[] { "AlumnoId", "CicloId" });

            migrationBuilder.CreateIndex(
                name: "IX_AlumnoAsignaciones_CicloId",
                table: "AlumnoAsignaciones",
                column: "CicloId");

            migrationBuilder.CreateIndex(
                name: "IX_AlumnoAsignaciones_ConceptoCobroId",
                table: "AlumnoAsignaciones",
                column: "ConceptoCobroId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlumnoAsignaciones");
        }
    }
}
