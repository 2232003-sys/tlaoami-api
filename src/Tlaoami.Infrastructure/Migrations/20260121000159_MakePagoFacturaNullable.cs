using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlaoami.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakePagoFacturaNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Facturas_FacturaId",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_FacturaId_IdempotencyKey",
                table: "Pagos");

            migrationBuilder.AlterColumn<Guid>(
                name: "FacturaId",
                table: "Pagos",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "AlumnoId",
                table: "Pagos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_FacturaId",
                table: "Pagos",
                column: "FacturaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_IdempotencyKey",
                table: "Pagos",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Facturas_FacturaId",
                table: "Pagos",
                column: "FacturaId",
                principalTable: "Facturas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Facturas_FacturaId",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_FacturaId",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_IdempotencyKey",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "AlumnoId",
                table: "Pagos");

            migrationBuilder.AlterColumn<Guid>(
                name: "FacturaId",
                table: "Pagos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_FacturaId_IdempotencyKey",
                table: "Pagos",
                columns: new[] { "FacturaId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Facturas_FacturaId",
                table: "Pagos",
                column: "FacturaId",
                principalTable: "Facturas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
