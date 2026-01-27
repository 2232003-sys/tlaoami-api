using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlaoami.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConceptoCobroTipoAplicaRecargo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AplicaRecargo",
                table: "ConceptosCobro",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TipoConcepto",
                table: "ConceptosCobro",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EscuelaSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EscuelaId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiaCorteColegiatura = table.Column<int>(type: "integer", nullable: false),
                    BloquearReinscripcionConSaldo = table.Column<bool>(type: "boolean", nullable: false),
                    ZonaHoraria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Moneda = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscuelaSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EscuelaSettings_EscuelaId",
                table: "EscuelaSettings",
                column: "EscuelaId",
                unique: true);

            // Defaults for existing records
            // Set Periodicidad = 'Mensual' where null to keep existing behavior
            migrationBuilder.Sql("UPDATE \"ConceptosCobro\" SET \"Periodicidad\" = 'Mensual' WHERE \"Periodicidad\" IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EscuelaSettings");

            migrationBuilder.DropColumn(
                name: "AplicaRecargo",
                table: "ConceptosCobro");

            migrationBuilder.DropColumn(
                name: "TipoConcepto",
                table: "ConceptosCobro");
        }
    }
}
