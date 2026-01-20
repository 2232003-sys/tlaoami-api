using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlaoami.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SalonId",
                table: "Grupos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Salones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Capacidad = table.Column<int>(type: "integer", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salones", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_SalonId",
                table: "Grupos",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_Salones_Codigo",
                table: "Salones",
                column: "Codigo",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Grupos_Salones_SalonId",
                table: "Grupos",
                column: "SalonId",
                principalTable: "Salones",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grupos_Salones_SalonId",
                table: "Grupos");

            migrationBuilder.DropTable(
                name: "Salones");

            migrationBuilder.DropIndex(
                name: "IX_Grupos_SalonId",
                table: "Grupos");

            migrationBuilder.DropColumn(
                name: "SalonId",
                table: "Grupos");
        }
    }
}
