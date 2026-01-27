using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlaoami.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPreConciliacionAfrimeMvp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosConciliacion_Alumnos_AlumnoId",
                table: "MovimientosConciliacion");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosConciliacion_Facturas_FacturaId",
                table: "MovimientosConciliacion");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosConciliacion_MovimientosBancarios_MovimientoBanc~",
                table: "MovimientosConciliacion");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosConciliacion_MovimientoBancarioId",
                table: "MovimientosConciliacion");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosBancarios_HashMovimiento",
                table: "MovimientosBancarios");

            migrationBuilder.DropColumn(
                name: "HashMovimiento",
                table: "MovimientosBancarios");

            migrationBuilder.AddColumn<Guid>(
                name: "AlumnoId1",
                table: "OrdenesVenta",
                type: "uuid",
                nullable: true);

            // Convert Tipo column from text to integer (idempotent)
            migrationBuilder.Sql(@"
DO $$
BEGIN
  IF EXISTS (
    SELECT 1
    FROM information_schema.columns
    WHERE table_name = 'MovimientosBancarios'
      AND column_name = 'Tipo'
      AND data_type = 'text'
  ) THEN
    ALTER TABLE ""MovimientosBancarios""
      ALTER COLUMN ""Tipo"" TYPE integer
      USING CASE
        WHEN ""Tipo"" = 'Deposito' THEN 1
        WHEN ""Tipo"" = 'Retiro' THEN 2
        ELSE 1
      END;
  END IF;
END $$;
");

            migrationBuilder.AlterColumn<decimal>(
                name: "Monto",
                table: "MovimientosBancarios",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            // Convert Estado column from text to integer (idempotent)
            migrationBuilder.Sql(@"
DO $$
BEGIN
  IF EXISTS (
    SELECT 1
    FROM information_schema.columns
    WHERE table_name = 'MovimientosBancarios'
      AND column_name = 'Estado'
      AND data_type = 'text'
  ) THEN
    ALTER TABLE ""MovimientosBancarios""
      ALTER COLUMN ""Estado"" TYPE integer
      USING CASE
        WHEN ""Estado"" = 'NoConciliado' THEN 1
        WHEN ""Estado"" = 'Conciliado' THEN 2
        WHEN ""Estado"" = 'Ignorado' THEN 3
        ELSE 1
      END;
  END IF;
END $$;
");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "MovimientosBancarios",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "MovimientosBancarios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CuentaBancariaId",
                table: "MovimientosBancarios",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EscuelaId",
                table: "MovimientosBancarios",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Estatus",
                table: "MovimientosBancarios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Folio",
                table: "MovimientosBancarios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HashUnico",
                table: "MovimientosBancarios",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ImportBatchId",
                table: "MovimientosBancarios",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenciaBanco",
                table: "MovimientosBancarios",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AlumnoId1",
                table: "AlumnoAsignaciones",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CuentasBancarias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EscuelaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Banco = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Alias = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Ultimos4 = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    Clabe = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: true),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasBancarias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PagosReportados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EscuelaId = table.Column<Guid>(type: "uuid", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaReportada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MontoReportado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MetodoPago = table.Column<int>(type: "integer", nullable: false),
                    ReferenciaTexto = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ComprobanteUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notas = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Estatus = table.Column<int>(type: "integer", nullable: false),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosReportados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagosReportados_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImportBancarioBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EscuelaId = table.Column<Guid>(type: "uuid", nullable: false),
                    CuentaBancariaId = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreArchivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PeriodoInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PeriodoFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalMovimientos = table.Column<int>(type: "integer", nullable: false),
                    TotalAbonos = table.Column<int>(type: "integer", nullable: false),
                    TotalCargos = table.Column<int>(type: "integer", nullable: false),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportBancarioBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportBancarioBatches_CuentasBancarias_CuentaBancariaId",
                        column: x => x.CuentaBancariaId,
                        principalTable: "CuentasBancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConciliacionMatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EscuelaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PagoReportadoId = table.Column<Guid>(type: "uuid", nullable: true),
                    MovimientoBancarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    ReglaMatch = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Estatus = table.Column<int>(type: "integer", nullable: false),
                    ConfirmadoPorUsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConfirmedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConciliacionMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConciliacionMatches_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConciliacionMatches_MovimientosBancarios_MovimientoBancario~",
                        column: x => x.MovimientoBancarioId,
                        principalTable: "MovimientosBancarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ConciliacionMatches_PagosReportados_PagoReportadoId",
                        column: x => x.PagoReportadoId,
                        principalTable: "PagosReportados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesVenta_AlumnoId1",
                table: "OrdenesVenta",
                column: "AlumnoId1");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosConciliacion_MovimientoBancarioId",
                table: "MovimientosConciliacion",
                column: "MovimientoBancarioId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosBancarios_CuentaBancariaId",
                table: "MovimientosBancarios",
                column: "CuentaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosBancarios_EscuelaId_Estado",
                table: "MovimientosBancarios",
                columns: new[] { "EscuelaId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosBancarios_FechaMovimiento_Monto",
                table: "MovimientosBancarios",
                columns: new[] { "Fecha", "Monto" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosBancarios_HashUnico_Unique",
                table: "MovimientosBancarios",
                column: "HashUnico",
                unique: true,
                filter: "\"HashUnico\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosBancarios_ImportBatchId",
                table: "MovimientosBancarios",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_AlumnoAsignaciones_AlumnoId1",
                table: "AlumnoAsignaciones",
                column: "AlumnoId1");

            migrationBuilder.CreateIndex(
                name: "IX_ConciliacionMatches_AlumnoId_Estatus",
                table: "ConciliacionMatches",
                columns: new[] { "AlumnoId", "Estatus" });

            migrationBuilder.CreateIndex(
                name: "IX_ConciliacionMatches_MovimientoBancarioId",
                table: "ConciliacionMatches",
                column: "MovimientoBancarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ConciliacionMatches_MovimientoBancarioId_Confirmado_Unique",
                table: "ConciliacionMatches",
                columns: new[] { "MovimientoBancarioId", "Estatus" },
                unique: true,
                filter: "\"Estatus\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ConciliacionMatches_PagoReportadoId",
                table: "ConciliacionMatches",
                column: "PagoReportadoId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasBancarias_EscuelaId_Activa",
                table: "CuentasBancarias",
                columns: new[] { "EscuelaId", "Activa" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_CuentaBancariaId",
                table: "ImportBancarioBatches",
                column: "CuentaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_EscuelaId_CreatedAtUtc",
                table: "ImportBancarioBatches",
                columns: new[] { "EscuelaId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PagosReportados_AlumnoId_Estatus",
                table: "PagosReportados",
                columns: new[] { "AlumnoId", "Estatus" });

            migrationBuilder.CreateIndex(
                name: "IX_PagosReportados_FechaReportada",
                table: "PagosReportados",
                column: "FechaReportada");

            migrationBuilder.CreateIndex(
                name: "IX_PagosReportados_MontoReportado_FechaReportada",
                table: "PagosReportados",
                columns: new[] { "MontoReportado", "FechaReportada" });

            migrationBuilder.AddForeignKey(
                name: "FK_AlumnoAsignaciones_Alumnos_AlumnoId1",
                table: "AlumnoAsignaciones",
                column: "AlumnoId1",
                principalTable: "Alumnos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosBancarios_CuentasBancarias_CuentaBancariaId",
                table: "MovimientosBancarios",
                column: "CuentaBancariaId",
                principalTable: "CuentasBancarias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosBancarios_ImportBancarioBatches_ImportBatchId",
                table: "MovimientosBancarios",
                column: "ImportBatchId",
                principalTable: "ImportBancarioBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosConciliacion_Alumnos_AlumnoId",
                table: "MovimientosConciliacion",
                column: "AlumnoId",
                principalTable: "Alumnos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosConciliacion_Facturas_FacturaId",
                table: "MovimientosConciliacion",
                column: "FacturaId",
                principalTable: "Facturas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosConciliacion_MovimientosBancarios_MovimientoBanc~",
                table: "MovimientosConciliacion",
                column: "MovimientoBancarioId",
                principalTable: "MovimientosBancarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesVenta_Alumnos_AlumnoId1",
                table: "OrdenesVenta",
                column: "AlumnoId1",
                principalTable: "Alumnos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlumnoAsignaciones_Alumnos_AlumnoId1",
                table: "AlumnoAsignaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosBancarios_CuentasBancarias_CuentaBancariaId",
                table: "MovimientosBancarios");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosBancarios_ImportBancarioBatches_ImportBatchId",
                table: "MovimientosBancarios");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosConciliacion_Alumnos_AlumnoId",
                table: "MovimientosConciliacion");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosConciliacion_Facturas_FacturaId",
                table: "MovimientosConciliacion");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosConciliacion_MovimientosBancarios_MovimientoBanc~",
                table: "MovimientosConciliacion");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesVenta_Alumnos_AlumnoId1",
                table: "OrdenesVenta");

            migrationBuilder.DropTable(
                name: "ConciliacionMatches");

            migrationBuilder.DropTable(
                name: "ImportBancarioBatches");

            migrationBuilder.DropTable(
                name: "PagosReportados");

            migrationBuilder.DropTable(
                name: "CuentasBancarias");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesVenta_AlumnoId1",
                table: "OrdenesVenta");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosConciliacion_MovimientoBancarioId",
                table: "MovimientosConciliacion");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosBancarios_CuentaBancariaId",
                table: "MovimientosBancarios");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosBancarios_EscuelaId_Estado",
                table: "MovimientosBancarios");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosBancarios_FechaMovimiento_Monto",
                table: "MovimientosBancarios");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosBancarios_HashUnico_Unique",
                table: "MovimientosBancarios");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosBancarios_ImportBatchId",
                table: "MovimientosBancarios");

            migrationBuilder.DropIndex(
                name: "IX_AlumnoAsignaciones_AlumnoId1",
                table: "AlumnoAsignaciones");

            migrationBuilder.DropColumn(
                name: "AlumnoId1",
                table: "OrdenesVenta");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "MovimientosBancarios");

            migrationBuilder.DropColumn(
                name: "CuentaBancariaId",
                table: "MovimientosBancarios");

            migrationBuilder.DropColumn(
                name: "EscuelaId",
                table: "MovimientosBancarios");

            migrationBuilder.DropColumn(
                name: "Estatus",
                table: "MovimientosBancarios");

            migrationBuilder.DropColumn(
                name: "Folio",
                table: "MovimientosBancarios");

            migrationBuilder.DropColumn(
                name: "HashUnico",
                table: "MovimientosBancarios");

            migrationBuilder.DropColumn(
                name: "ImportBatchId",
                table: "MovimientosBancarios");

            migrationBuilder.DropColumn(
                name: "ReferenciaBanco",
                table: "MovimientosBancarios");

            migrationBuilder.DropColumn(
                name: "AlumnoId1",
                table: "AlumnoAsignaciones");

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "MovimientosBancarios",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "Monto",
                table: "MovimientosBancarios",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "MovimientosBancarios",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "MovimientosBancarios",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "HashMovimiento",
                table: "MovimientosBancarios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosConciliacion_MovimientoBancarioId",
                table: "MovimientosConciliacion",
                column: "MovimientoBancarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosBancarios_HashMovimiento",
                table: "MovimientosBancarios",
                column: "HashMovimiento",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosConciliacion_Alumnos_AlumnoId",
                table: "MovimientosConciliacion",
                column: "AlumnoId",
                principalTable: "Alumnos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosConciliacion_Facturas_FacturaId",
                table: "MovimientosConciliacion",
                column: "FacturaId",
                principalTable: "Facturas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosConciliacion_MovimientosBancarios_MovimientoBanc~",
                table: "MovimientosConciliacion",
                column: "MovimientoBancarioId",
                principalTable: "MovimientosBancarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
