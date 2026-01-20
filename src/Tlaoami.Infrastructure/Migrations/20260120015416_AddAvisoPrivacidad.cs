using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlaoami.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAvisoPrivacidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AvisosPrivacidad",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Contenido = table.Column<string>(type: "text", nullable: false),
                    Vigente = table.Column<bool>(type: "boolean", nullable: false),
                    PublicadoEnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvisosPrivacidad", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AceptacionesAvisoPrivacidad",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AvisoPrivacidadId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    AceptadoEnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AceptacionesAvisoPrivacidad", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AceptacionesAvisoPrivacidad_AvisosPrivacidad_AvisoPrivacida~",
                        column: x => x.AvisoPrivacidadId,
                        principalTable: "AvisosPrivacidad",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AceptacionesAvisoPrivacidad_Users_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AceptacionesAvisoPrivacidad_AvisoPrivacidadId",
                table: "AceptacionesAvisoPrivacidad",
                column: "AvisoPrivacidadId");

            migrationBuilder.CreateIndex(
                name: "IX_AceptacionesAvisoPrivacidad_UsuarioId_AvisoPrivacidadId",
                table: "AceptacionesAvisoPrivacidad",
                columns: new[] { "UsuarioId", "AvisoPrivacidadId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AvisosPrivacidad_Vigente",
                table: "AvisosPrivacidad",
                column: "Vigente",
                unique: true);
                // Note: partial index (filter: "Vigente = true") is supported on Postgres 
                // For SQLite compatibility, removed filter clause. Application enforces uniqueness.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AceptacionesAvisoPrivacidad");

            migrationBuilder.DropTable(
                name: "AvisosPrivacidad");
        }
    }
}
