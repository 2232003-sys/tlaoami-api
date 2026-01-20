using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlaoami.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReinscripcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reinscripciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CicloOrigenId = table.Column<Guid>(type: "uuid", nullable: true),
                    GrupoOrigenId = table.Column<Guid>(type: "uuid", nullable: true),
                    CicloDestinoId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrupoDestinoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MotivoBloqueo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SaldoAlMomento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reinscripciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reinscripciones_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reinscripciones_CiclosEscolares_CicloDestinoId",
                        column: x => x.CicloDestinoId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reinscripciones_CiclosEscolares_CicloOrigenId",
                        column: x => x.CicloOrigenId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Reinscripciones_Grupos_GrupoDestinoId",
                        column: x => x.GrupoDestinoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reinscripciones_Grupos_GrupoOrigenId",
                        column: x => x.GrupoOrigenId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reinscripciones_AlumnoId_CicloDestinoId",
                table: "Reinscripciones",
                columns: new[] { "AlumnoId", "CicloDestinoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reinscripciones_CicloDestinoId",
                table: "Reinscripciones",
                column: "CicloDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_Reinscripciones_CicloOrigenId",
                table: "Reinscripciones",
                column: "CicloOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_Reinscripciones_GrupoDestinoId",
                table: "Reinscripciones",
                column: "GrupoDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_Reinscripciones_GrupoOrigenId",
                table: "Reinscripciones",
                column: "GrupoOrigenId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reinscripciones");
        }
    }
}
