using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlaoami.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandEscuelaSettingsWithInstitutionalInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosBancarios_CuentasBancarias_CuentaBancariaId",
                table: "MovimientosBancarios");

            migrationBuilder.AlterColumn<Guid>(
                name: "CuentaBancariaId",
                table: "MovimientosBancarios",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "EscuelaSettings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "EscuelaSettings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "EscuelaSettings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "EscuelaSettings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RazonSocial",
                table: "EscuelaSettings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "EscuelaSettings",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TextoRecibos",
                table: "EscuelaSettings",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosBancarios_CuentasBancarias_CuentaBancariaId",
                table: "MovimientosBancarios",
                column: "CuentaBancariaId",
                principalTable: "CuentasBancarias",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosBancarios_CuentasBancarias_CuentaBancariaId",
                table: "MovimientosBancarios");

            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "EscuelaSettings");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "EscuelaSettings");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "EscuelaSettings");

            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "EscuelaSettings");

            migrationBuilder.DropColumn(
                name: "RazonSocial",
                table: "EscuelaSettings");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "EscuelaSettings");

            migrationBuilder.DropColumn(
                name: "TextoRecibos",
                table: "EscuelaSettings");

            migrationBuilder.AlterColumn<Guid>(
                name: "CuentaBancariaId",
                table: "MovimientosBancarios",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosBancarios_CuentasBancarias_CuentaBancariaId",
                table: "MovimientosBancarios",
                column: "CuentaBancariaId",
                principalTable: "CuentasBancarias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
